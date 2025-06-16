using ExpenseTracker.Models.Dto;
using ExpenseTracker.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{

    private readonly IReportService _reportService;

    private readonly IDashboardService _dashboardService;
    public DashboardController(IReportService reportService, IDashboardService dashboardService)
    {
        _reportService = reportService;
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    // [Authorize(Roles = "Admin")]
    public IActionResult GetExpenseSummary([FromQuery] CsvExportFilterDto csvExportFilterDto)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Daily report validations
        if (csvExportFilterDto.ReportType == "daily")
        {
            if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.StartDate > today)
                return BadRequest("Start date cannot be in the future.");

            if (csvExportFilterDto.EndDate.HasValue && csvExportFilterDto.EndDate > today)
                return BadRequest("End date cannot be in the future.");

            if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.EndDate.HasValue &&
                csvExportFilterDto.StartDate > csvExportFilterDto.EndDate)
                return BadRequest("Start date cannot be greater than end date.");
        }

        // Monthly report validations
        if (csvExportFilterDto.ReportType == "monthly" && csvExportFilterDto.RangeType == "custome")
        {
            bool startValid = csvExportFilterDto.StartMonth.HasValue && csvExportFilterDto.StartYear.HasValue;
            bool endValid = csvExportFilterDto.EndMonth.HasValue && csvExportFilterDto.EndYear.HasValue;

            if (startValid && endValid)
            {
                DateOnly start = new DateOnly(csvExportFilterDto.StartYear!.Value, csvExportFilterDto.StartMonth!.Value, 1);
                DateOnly end = new DateOnly(csvExportFilterDto.EndYear!.Value, csvExportFilterDto.EndMonth!.Value,
                    DateTime.DaysInMonth(csvExportFilterDto.EndYear.Value, csvExportFilterDto.EndMonth.Value));

                if (start > today)
                    return BadRequest("Start month cannot be in the future.");

                if (end > today)
                    return BadRequest("End month cannot be in the future.");

                if (start > end)
                    return BadRequest("Start month cannot be greater than end month.");
            }
            else
            {
                return BadRequest("Start and end month/year must be provided for custom monthly reports.");
            }
        }

        var summary = _dashboardService.GetExpenseSummary(csvExportFilterDto);
        return Ok(summary);
    }



    [HttpGet("export-csv")]
    // [Authorize(Roles = "Admin")]
    public IActionResult ExportExpensesToCsv([FromQuery] CsvExportFilterDto csvExportFilterDto)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Daily report validations
        if (csvExportFilterDto.ReportType == "daily")
        {
            if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.StartDate > today)
                return BadRequest("Start date cannot be in the future.");

            if (csvExportFilterDto.EndDate.HasValue && csvExportFilterDto.EndDate > today)
                return BadRequest("End date cannot be in the future.");

            if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.EndDate.HasValue &&
                csvExportFilterDto.StartDate > csvExportFilterDto.EndDate)
                return BadRequest("Start date cannot be greater than end date.");
        }

        // Monthly report validations (custom only)
        if (csvExportFilterDto.ReportType == "monthly" && csvExportFilterDto.RangeType == "custome")
        {
            bool startValid = csvExportFilterDto.StartMonth.HasValue && csvExportFilterDto.StartYear.HasValue;
            bool endValid = csvExportFilterDto.EndMonth.HasValue && csvExportFilterDto.EndYear.HasValue;

            if (startValid && endValid)
            {
                DateOnly start = new DateOnly(csvExportFilterDto.StartYear!.Value, csvExportFilterDto.StartMonth!.Value, 1);
                DateOnly end = new DateOnly(csvExportFilterDto.EndYear!.Value, csvExportFilterDto.EndMonth!.Value,
                    DateTime.DaysInMonth(csvExportFilterDto.EndYear.Value, csvExportFilterDto.EndMonth.Value));

                if (start > today)
                    return BadRequest("Start month cannot be in the future.");

                if (end > today)
                    return BadRequest("End month cannot be in the future.");

                if (start > end)
                    return BadRequest("Start month cannot be greater than end month.");
            }
            else
            {
                return BadRequest("Start and end month/year must be provided for custom monthly reports.");
            }
        }

        // Export if all validations pass
        MemoryStream fileStream = _dashboardService.ExportExpensesToCsv(csvExportFilterDto);
        return File(fileStream.ToArray(), "text/csv", "Expense_Report.csv");
    }


}
