using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IDashboardService
{

    public MemoryStream ExportExpensesToCsv(CsvExportFilterRequestDto csvExportFilterRequestDto);

    public object GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto);
}
