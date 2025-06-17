using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
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
                return NotFound(ErrorMessages.UserNotFound);
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                if (userCsvExportFilterRequestDto.StartDate > today || userCsvExportFilterRequestDto.EndDate > today)
                {
                    return BadRequest(ErrorMessages.FutureDateNotAllowed);
                }

                if (userCsvExportFilterRequestDto.StartDate > userCsvExportFilterRequestDto.EndDate)
                {
                    return BadRequest(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Monthly && userCsvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                if (userCsvExportFilterRequestDto.StartMonth.HasValue && userCsvExportFilterRequestDto.StartYear.HasValue &&
                    userCsvExportFilterRequestDto.EndMonth.HasValue && userCsvExportFilterRequestDto.EndYear.HasValue)
                {
                    DateOnly start = new DateOnly(userCsvExportFilterRequestDto.StartYear.Value, userCsvExportFilterRequestDto.StartMonth.Value, 1);
                    DateOnly end = new DateOnly(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value,
                        DateTime.DaysInMonth(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value));

                    if (start > today)
                    {
                        return BadRequest(ErrorMessages.FutureMonthNotAllowed);
                    }
                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        return BadRequest(ErrorMessages.EndMonthInFuture);
                    }

                    if (start > end)
                    {
                        return BadRequest(ErrorMessages.FutureMonthNotAllowed);
                    }
                }
                else
                {
                    return BadRequest(ErrorMessages.CustomMonthRangeRequired);
                }
            }

            MemoryStream? fileStream = _reportService.ExportUserExpensesToCsv(userId ?? 0, userCsvExportFilterRequestDto);
            return File(fileStream.ToArray(), "text/csv", "My_Expense_Report.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.GetSummaryFailed, detail = ex.Message });
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
                return NotFound(ErrorMessages.UserNotFound);
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                if (userCsvExportFilterRequestDto.StartDate > today || userCsvExportFilterRequestDto.EndDate > today)
                {
                    return BadRequest(ErrorMessages.FutureDateNotAllowed);
                }

                if (userCsvExportFilterRequestDto.StartDate > userCsvExportFilterRequestDto.EndDate)
                {
                    return BadRequest(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (userCsvExportFilterRequestDto.ReportType == ReportType.Monthly && userCsvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                if (userCsvExportFilterRequestDto.StartMonth.HasValue && userCsvExportFilterRequestDto.StartYear.HasValue &&
                    userCsvExportFilterRequestDto.EndMonth.HasValue && userCsvExportFilterRequestDto.EndYear.HasValue)
                {
                    DateOnly start = new DateOnly(userCsvExportFilterRequestDto.StartYear.Value, userCsvExportFilterRequestDto.StartMonth.Value, 1);
                    DateOnly end = new DateOnly(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value,
                        DateTime.DaysInMonth(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value));

                    if (start > today)
                    {
                        return BadRequest(ErrorMessages.FutureMonthNotAllowed);
                    }
                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        return BadRequest(ErrorMessages.EndMonthInFuture);
                    }
                    if (start > end)
                    {
                        return BadRequest(ErrorMessages.FutureMonthNotAllowed);
                    }
                }
                else
                {
                    return BadRequest(ErrorMessages.CustomMonthRangeRequired);
                }
            }

            var summary = _reportService.GetUserExpenseSummary(userId.Value, userCsvExportFilterRequestDto);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.GetSummaryFailed, detail = ex.Message });
        }
    }
}
