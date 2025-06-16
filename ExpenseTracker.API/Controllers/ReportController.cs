using System.Security.Claims;
using ExpenseTracker.Models.Dto;
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
    public IActionResult ExportMyExpensesToCsv([FromQuery] UserCsvExportFilterDto filterDto)
    {
        if (!User.Identity!.IsAuthenticated)
        {
            return NotFound("User not found!");
        }

        int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        if (userId == 0)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (filterDto.ReportType == "daily")
        {
            if (filterDto.StartDate > today || filterDto.EndDate > today)
                return BadRequest("Dates cannot be in the future.");

            if (filterDto.StartDate > filterDto.EndDate)
                return BadRequest("Start date cannot be after end date.");
        }

        if (filterDto.ReportType == "monthly" && filterDto.RangeType == "custom")
        {
            if (filterDto.StartMonth.HasValue && filterDto.StartYear.HasValue &&
                filterDto.EndMonth.HasValue && filterDto.EndYear.HasValue)
            {
                var start = new DateOnly(filterDto.StartYear.Value, filterDto.StartMonth.Value, 1);
                var end = new DateOnly(filterDto.EndYear.Value, filterDto.EndMonth.Value,
                    DateTime.DaysInMonth(filterDto.EndYear.Value, filterDto.EndMonth.Value));

                if (start > today || end > today)
                    return BadRequest("Months cannot be in the future.");

                if (start > end)
                    return BadRequest("Start month cannot be after end month.");
            }
            else
            {
                return BadRequest("Start and end month/year must be provided.");
            }
        }

        var fileStream = _reportService.ExportUserExpensesToCsv(userId ?? 0, filterDto);
        return File(fileStream.ToArray(), "text/csv", "My_Expense_Report.csv");
    }

    [HttpGet("my-summary")]
    [Authorize(Roles = "User")]
    public IActionResult GetMyExpenseSummary([FromQuery] UserCsvExportFilterDto filterDto)
    {
        if (!User.Identity!.IsAuthenticated)
            return NotFound("User not found!");

        int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (filterDto.ReportType == "daily")
        {
            if (filterDto.StartDate > today || filterDto.EndDate > today)
                return BadRequest("Dates cannot be in the future.");
            if (filterDto.StartDate > filterDto.EndDate)
                return BadRequest("Start date cannot be after end date.");
        }

        if (filterDto.ReportType == "monthly" && filterDto.RangeType == "custom")
        {
            if (filterDto.StartMonth.HasValue && filterDto.StartYear.HasValue &&
                filterDto.EndMonth.HasValue && filterDto.EndYear.HasValue)
            {
                var start = new DateOnly(filterDto.StartYear.Value, filterDto.StartMonth.Value, 1);
                var end = new DateOnly(filterDto.EndYear.Value, filterDto.EndMonth.Value,
                    DateTime.DaysInMonth(filterDto.EndYear.Value, filterDto.EndMonth.Value));

                if (start > today || end > today)
                    return BadRequest("Months cannot be in the future.");
                if (start > end)
                    return BadRequest("Start month cannot be after end month.");
            }
            else
            {
                return BadRequest("Start and end month/year must be provided.");
            }
        }

        var summary = _reportService.GetUserExpenseSummary(userId.Value, filterDto);
        return Ok(summary);
    }




}
