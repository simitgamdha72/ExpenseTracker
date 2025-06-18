using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IDashboardService
{

    public MemoryStream ExportExpensesToCsv(CsvExportFilterRequestDto csvExportFilterRequestDto, int? userId);

    public object GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto);
}
