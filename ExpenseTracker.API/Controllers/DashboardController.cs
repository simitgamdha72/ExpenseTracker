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
        Response<object?>? result = _dashboardService.GetExpenseSummary(csvExportFilterRequestDto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("export-csv")]
    [Authorize(Roles = "Admin")]
    public IActionResult ExportExpensesToCsv([FromQuery] CsvExportFilterRequestDto filter)
    {
        Response<object?>? result = _dashboardService.ExportExpensesToCsv(filter, User);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result);

        MemoryStream? stream = (MemoryStream)result.Data!;
        return File(stream.ToArray(), "text/csv", "Expense_Report.csv");
    }


}
