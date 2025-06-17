using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IReportService
{
    public MemoryStream ExportUserExpensesToCsv(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto);

    public object GetUserExpenseSummary(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto);

}
