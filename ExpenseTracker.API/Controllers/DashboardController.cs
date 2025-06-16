using ExpenseTracker.Models.Dto;
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
    public IActionResult GetExpenseSummary([FromQuery] CsvExportFilterDto csvExportFilterDto)
    {
        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (csvExportFilterDto.ReportType == "daily")
            {
                if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.StartDate > today)
                {
                    return BadRequest(ErrorMessages.StartDateInFuture);
                }

                if (csvExportFilterDto.EndDate.HasValue && csvExportFilterDto.EndDate > today)
                {
                    return BadRequest(ErrorMessages.EndDateInFuture);
                }

                if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.EndDate.HasValue &&
                        csvExportFilterDto.StartDate > csvExportFilterDto.EndDate)
                {
                    return BadRequest(ErrorMessages.StartDateAfterEndDate);
                }
            }

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
                    {
                        return BadRequest(ErrorMessages.StartMonthInFuture);
                    }

                    if (end > today)
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

            var summary = _dashboardService.GetExpenseSummary(csvExportFilterDto);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.GetSummaryFailed, detail = ex.Message });
        }
    }



    [HttpGet("export-csv")]
    [Authorize(Roles = "Admin")]
    public IActionResult ExportExpensesToCsv([FromQuery] CsvExportFilterDto csvExportFilterDto)
    {
        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (csvExportFilterDto.ReportType == "daily")
            {
                if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.StartDate > today)
                {
                    return BadRequest(ErrorMessages.StartDateInFuture);
                }

                if (csvExportFilterDto.EndDate.HasValue && csvExportFilterDto.EndDate > today)
                {
                    return BadRequest(ErrorMessages.EndDateInFuture);
                }

                if (csvExportFilterDto.StartDate.HasValue && csvExportFilterDto.EndDate.HasValue &&
                        csvExportFilterDto.StartDate > csvExportFilterDto.EndDate)
                {
                    return BadRequest(ErrorMessages.StartDateAfterEndDate);
                }
            }

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
                    {
                        return BadRequest(ErrorMessages.StartMonthInFuture);
                    }

                    if (end > today)
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

            MemoryStream fileStream = _dashboardService.ExportExpensesToCsv(csvExportFilterDto);
            return File(fileStream.ToArray(), "text/csv", "Expense_Report.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.ExportCsvFailed, detail = ex.Message });
        }
    }


}
