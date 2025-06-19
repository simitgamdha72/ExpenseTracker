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
public class ReportController : ControllerBase
{

    private readonly IReportService _reportService;
    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("my-report-csv")]
    [Authorize(Roles = "User")]
    public IActionResult ExportMyExpensesToCsv([FromQuery] UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
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

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                if (userCsvExportFilterRequestDto.StartDate > today || userCsvExportFilterRequestDto.EndDate > today)
                {
                    responseError.Errors = new[] { ErrorMessages.FutureDateNotAllowed };
                    return BadRequest(responseError);
                }

                if (userCsvExportFilterRequestDto.StartDate > userCsvExportFilterRequestDto.EndDate)
                {
                    responseError.Errors = new[] { ErrorMessages.StartDateAfterEndDate };
                    return BadRequest(responseError);

                }
            }

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Monthly && userCsvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                if (userCsvExportFilterRequestDto.StartMonth.HasValue && userCsvExportFilterRequestDto.StartYear.HasValue &&
                    userCsvExportFilterRequestDto.EndMonth.HasValue && userCsvExportFilterRequestDto.EndYear.HasValue)
                {
                    DateOnly start = new DateOnly(userCsvExportFilterRequestDto.StartYear.Value, userCsvExportFilterRequestDto.StartMonth.Value, 1);
                    DateOnly end = new DateOnly(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value,
                        DateTime.DaysInMonth(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value));

                    if (start > today)
                    {
                        responseError.Errors = new[] { ErrorMessages.FutureMonthNotAllowed };
                        return BadRequest(responseError);
                    }
                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        responseError.Errors = new[] { ErrorMessages.EndMonthInFuture };
                        return BadRequest(responseError);
                    }

                    if (start > end)
                    {
                        responseError.Errors = new[] { ErrorMessages.FutureMonthNotAllowed };
                        return BadRequest(responseError);
                    }
                }
                else
                {
                    responseError.Errors = new[] { ErrorMessages.CustomMonthRangeRequired };
                    return BadRequest(responseError);
                }
            }

            MemoryStream? fileStream = _reportService.ExportUserExpensesToCsv(userId ?? 0, userCsvExportFilterRequestDto);
            return File(fileStream.ToArray(), "text/csv", "My_Expense_Report.csv");
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

    [HttpGet("my-summary")]
    [Authorize(Roles = "User")]
    public IActionResult GetMyExpenseSummary([FromQuery] UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
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

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                if (userCsvExportFilterRequestDto.StartDate > today || userCsvExportFilterRequestDto.EndDate > today)
                {
                    responseError.Errors = new[] { ErrorMessages.FutureDateNotAllowed };
                    return BadRequest(responseError);
                }

                if (userCsvExportFilterRequestDto.StartDate > userCsvExportFilterRequestDto.EndDate)
                {
                    responseError.Errors = new[] { ErrorMessages.StartDateAfterEndDate };
                    return BadRequest(responseError);
                }
            }

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Monthly && userCsvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                };

                if (userCsvExportFilterRequestDto.StartMonth.HasValue && userCsvExportFilterRequestDto.StartYear.HasValue &&
                    userCsvExportFilterRequestDto.EndMonth.HasValue && userCsvExportFilterRequestDto.EndYear.HasValue)
                {
                    DateOnly start = new DateOnly(userCsvExportFilterRequestDto.StartYear.Value, userCsvExportFilterRequestDto.StartMonth.Value, 1);
                    DateOnly end = new DateOnly(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value,
                        DateTime.DaysInMonth(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value));

                    if (start > today)
                    {
                        responseError.Errors = new[] { ErrorMessages.FutureMonthNotAllowed };
                        return BadRequest(responseError);
                    }
                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        responseError.Errors = new[] { ErrorMessages.EndMonthInFuture };
                        return BadRequest(responseError);
                    }
                    if (start > end)
                    {
                        responseError.Errors = new[] { ErrorMessages.FutureMonthNotAllowed };
                        return BadRequest(responseError);
                    }
                }
                else
                {
                    responseError.Errors = new[] { ErrorMessages.CustomMonthRangeRequired };
                    return BadRequest(responseError);
                }
            }

            var summary = _reportService.GetUserExpenseSummary(userId.Value, userCsvExportFilterRequestDto);

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
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };

            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}
