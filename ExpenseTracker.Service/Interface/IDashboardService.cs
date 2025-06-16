using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IDashboardService
{

    public MemoryStream ExportExpensesToCsv(CsvExportFilterDto csvExportFilterDto);

    public object GetExpenseSummary(CsvExportFilterDto csvExportFilterDto);
}
