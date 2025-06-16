using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IReportService
{
    public MemoryStream ExportUserExpensesToCsv(int userId, UserCsvExportFilterDto filter);

    public object GetUserExpenseSummary(int userId, UserCsvExportFilterDto filter);

}
