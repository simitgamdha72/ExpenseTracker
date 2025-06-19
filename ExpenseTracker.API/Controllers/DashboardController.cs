using System.Net;
using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{

    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetExpenseSummary([FromQuery] CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        try
        {

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly currentMonthStart = new DateOnly(today.Year, today.Month, 1);

            if (csvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.StartDate > today)
                {
                    responseError.Errors = new[] { ErrorMessages.StartDateInFuture };
                    return BadRequest(responseError);
                }

                if (csvExportFilterRequestDto.EndDate.HasValue && csvExportFilterRequestDto.EndDate > today)
                {
                    responseError.Errors = new[] { ErrorMessages.EndDateInFuture };
                    return BadRequest(responseError);
                }

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue &&
                        csvExportFilterRequestDto.StartDate > csvExportFilterRequestDto.EndDate)
                {
                    responseError.Errors = new[] { ErrorMessages.StartDateAfterEndDate };
                    return BadRequest(responseError);
                }
            }

            if (csvExportFilterRequestDto.ReportType == ReportType.Monthly && csvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                bool startValid = csvExportFilterRequestDto.StartMonth.HasValue && csvExportFilterRequestDto.StartYear.HasValue;
                bool endValid = csvExportFilterRequestDto.EndMonth.HasValue && csvExportFilterRequestDto.EndYear.HasValue;

                if (startValid && endValid)
                {
                    DateOnly start = new DateOnly(csvExportFilterRequestDto.StartYear!.Value, csvExportFilterRequestDto.StartMonth!.Value, 1);
                    DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear!.Value, csvExportFilterRequestDto.EndMonth!.Value,
                        DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));


                    if (start > today)
                    {
                        responseError.Errors = new[] { ErrorMessages.StartMonthInFuture };
                        return BadRequest(responseError);
                    }

                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        responseError.Errors = new[] { ErrorMessages.EndMonthInFuture };
                        return BadRequest(responseError);

                    }

                    if (start > end)
                    {
                        responseError.Errors = new[] { ErrorMessages.StartMonthAfterEndMonth };
                        return BadRequest(responseError);
                    }
                }
                else
                {
                    responseError.Errors = new[] { ErrorMessages.CustomMonthRangeRequired };
                    return BadRequest(responseError);
                }

            }

            var summary = _dashboardService.GetExpenseSummary(csvExportFilterRequestDto);

            Response<object> response = new Response<object>
            {
                Message = SuccessMessages.SummaryDataFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = summary
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.GetSummaryFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Data = null,
                Errors = new[] { ex.Message }
            };

            return BadRequest(response);
        }
    }

    [HttpGet("export-csv")]
    [Authorize(Roles = "Admin")]
    public IActionResult ExportExpensesToCsv([FromQuery] CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        try
        {
            if (!User.Identity!.IsAuthenticated)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
                return NotFound(responseError);
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
                return NotFound(responseError);
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (csvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.StartDate > today)
                {
                    responseError.Errors = new[] { ErrorMessages.StartDateInFuture };
                    return BadRequest(responseError);
                }

                if (csvExportFilterRequestDto.EndDate.HasValue && csvExportFilterRequestDto.EndDate > today)
                {
                    responseError.Errors = new[] { ErrorMessages.EndDateInFuture };
                    return BadRequest(responseError);
                }

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue &&
                        csvExportFilterRequestDto.StartDate > csvExportFilterRequestDto.EndDate)
                {
                    responseError.Errors = new[] { ErrorMessages.StartDateAfterEndDate };
                    return BadRequest(responseError);
                }
            }

            if (csvExportFilterRequestDto.ReportType == ReportType.Monthly && csvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                bool startValid = csvExportFilterRequestDto.StartMonth.HasValue && csvExportFilterRequestDto.StartYear.HasValue;
                bool endValid = csvExportFilterRequestDto.EndMonth.HasValue && csvExportFilterRequestDto.EndYear.HasValue;

                if (startValid && endValid)
                {
                    DateOnly start = new DateOnly(csvExportFilterRequestDto.StartYear!.Value, csvExportFilterRequestDto.StartMonth!.Value, 1);
                    DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear!.Value, csvExportFilterRequestDto.EndMonth!.Value,
                        DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));

                    if (start > today)
                    {
                        responseError.Errors = new[] { ErrorMessages.StartMonthInFuture };
                        return BadRequest(responseError);
                    }

                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        responseError.Errors = new[] { ErrorMessages.EndMonthInFuture };
                        return BadRequest(responseError);
                    }

                    if (start > end)
                    {
                        responseError.Errors = new[] { ErrorMessages.StartMonthAfterEndMonth };
                        return BadRequest(responseError);
                    }
                }
                else
                {
                    responseError.Errors = new[] { ErrorMessages.CustomMonthRangeRequired };
                    return BadRequest(responseError);
                }

            }

            MemoryStream fileStream = _dashboardService.ExportExpensesToCsv(csvExportFilterRequestDto, userId);
            return File(fileStream.ToArray(), "text/csv", "Expense_Report.csv");
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.ExportCsvFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Data = null,
                Errors = new[] { ex.Message }
            };

            return BadRequest(response);
        }
    }
}
