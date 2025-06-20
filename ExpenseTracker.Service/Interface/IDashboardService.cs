using System.Security.Claims;
using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IDashboardService
{
    Response<object?> ExportExpensesToCsv(CsvExportFilterRequestDto dto, ClaimsPrincipal user);
    Response<object?> GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto);
}
