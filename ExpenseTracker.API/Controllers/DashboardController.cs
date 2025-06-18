using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
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
                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.StartDate > today)
                {
                    return BadRequest(ErrorMessages.StartDateInFuture);
                }

                if (csvExportFilterRequestDto.EndDate.HasValue && csvExportFilterRequestDto.EndDate > today)
                {
                    return BadRequest(ErrorMessages.EndDateInFuture);
                }

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue &&
                        csvExportFilterRequestDto.StartDate > csvExportFilterRequestDto.EndDate)
                {
                    return BadRequest(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (csvExportFilterRequestDto.ReportType == ReportType.Monthly && csvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                bool startValid = csvExportFilterRequestDto.StartMonth.HasValue && csvExportFilterRequestDto.StartYear.HasValue;
                bool endValid = csvExportFilterRequestDto.EndMonth.HasValue && csvExportFilterRequestDto.EndYear.HasValue;

                if (startValid && endValid)
                {
                    DateOnly start = new DateOnly(csvExportFilterRequestDto.StartYear!.Value, csvExportFilterRequestDto.StartMonth!.Value, 1);
                    DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear!.Value, csvExportFilterRequestDto.EndMonth!.Value,
                        DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));


                    if (start > today)
                    {
                        return BadRequest(ErrorMessages.StartMonthInFuture);
                    }

                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        return BadRequest(ErrorMessages.EndMonthInFuture);
                    }

                    if (start > end)
                    {
                        return BadRequest(ErrorMessages.StartMonthAfterEndMonth);
                    }
                }
                else
                {
                    return BadRequest(ErrorMessages.CustomMonthRangeRequired);
                }

            }

            var summary = _dashboardService.GetExpenseSummary(csvExportFilterRequestDto);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.GetSummaryFailed, detail = ex.Message });
        }
    }

    [HttpGet("export-csv")]
    [Authorize(Roles = "Admin")]
    public IActionResult ExportExpensesToCsv([FromQuery] CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (csvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.StartDate > today)
                {
                    return BadRequest(ErrorMessages.StartDateInFuture);
                }

                if (csvExportFilterRequestDto.EndDate.HasValue && csvExportFilterRequestDto.EndDate > today)
                {
                    return BadRequest(ErrorMessages.EndDateInFuture);
                }

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue &&
                        csvExportFilterRequestDto.StartDate > csvExportFilterRequestDto.EndDate)
                {
                    return BadRequest(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (csvExportFilterRequestDto.ReportType == ReportType.Monthly && csvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                bool startValid = csvExportFilterRequestDto.StartMonth.HasValue && csvExportFilterRequestDto.StartYear.HasValue;
                bool endValid = csvExportFilterRequestDto.EndMonth.HasValue && csvExportFilterRequestDto.EndYear.HasValue;

                if (startValid && endValid)
                {
                    DateOnly start = new DateOnly(csvExportFilterRequestDto.StartYear!.Value, csvExportFilterRequestDto.StartMonth!.Value, 1);
                    DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear!.Value, csvExportFilterRequestDto.EndMonth!.Value,
                        DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));

                    if (start > today)
                    {
                        return BadRequest(ErrorMessages.StartMonthInFuture);
                    }

                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        return BadRequest(ErrorMessages.EndMonthInFuture);
                    }

                    if (start > end)
                    {
                        return BadRequest(ErrorMessages.StartMonthAfterEndMonth);
                    }
                }
                else
                {
                    return BadRequest(ErrorMessages.CustomMonthRangeRequired);
                }

            }

            MemoryStream fileStream = _dashboardService.ExportExpensesToCsv(csvExportFilterRequestDto);
            return File(fileStream.ToArray(), "text/csv", "Expense_Report.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.ExportCsvFailed, detail = ex.Message });
        }
    }
}
