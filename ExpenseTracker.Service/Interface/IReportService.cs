using System.Security.Claims;
using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IReportService
{

    (MemoryStream FileStream, Response<object> Response) ExportUserExpensesToCsv(ClaimsPrincipal user, UserCsvExportFilterRequestDto filterDto);

    Response<object> GetUserExpenseSummary(ClaimsPrincipal user, UserCsvExportFilterRequestDto filterDto);

}
