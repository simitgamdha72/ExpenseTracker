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
    public IActionResult ExportMyExpensesToCsv([FromQuery] UserCsvExportFilterRequestDto filterDto)
    {
        var (fileStream, response) = _reportService.ExportUserExpensesToCsv(User, filterDto);

        if (!response.Succeeded)
        {
            return StatusCode(response.StatusCode, response);
        }

        return File(fileStream.ToArray(), "text/csv", "My_Expense_Report.csv");
    }


    [HttpGet("my-summary")]
    [Authorize(Roles = "User")]
    public IActionResult GetMyExpenseSummary([FromQuery] UserCsvExportFilterRequestDto filterDto)
    {
        Response<object>? response = _reportService.GetUserExpenseSummary(User, filterDto);
        return StatusCode(response.StatusCode, response);
    }

}
