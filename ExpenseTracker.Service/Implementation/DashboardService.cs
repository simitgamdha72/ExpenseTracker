using System.Security.Claims;
using System.Text;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;

namespace ExpenseTracker.Service.Implementation;

public class DashboardService : IDashboardService
{

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseReportRepository _expenseReportRepository;

    public DashboardService(IExpenseRepository expenseRepository, IExpenseReportRepository expenseReportRepository)
    {
        _expenseRepository = expenseRepository;
        _expenseReportRepository = expenseReportRepository;
    }

    public object GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredExpenses(csvExportFilterRequestDto);
        bool isMonthly = csvExportFilterRequestDto.ReportType == ReportType.Monthly;

        var groupedByCategory = expenses
            .GroupBy(exp => exp.Category?.Name ?? "Uncategorized")
            .Select(group => new
            {
                Category = group.Key,
                TotalAmount = group.Sum(e => e.Amount),
                Expenses = group.Select(e => new
                {
                    Username = e.User?.Username,
                    Date = isMonthly
                        ? e.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                        : e.ExpenseDate.ToString("yyyy-MM-dd"),
                    Amount = e.Amount,
                    Note = e.Note
                }).ToList()
            }).ToList();

        decimal grandTotal = groupedByCategory.Sum(g => g.TotalAmount);

        return new
        {
            Categories = groupedByCategory,
            TotalExpense = grandTotal
        };
    }


    public MemoryStream ExportExpensesToCsv(CsvExportFilterRequestDto csvExportFilterRequestDto, int? userId)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredExpenses(csvExportFilterRequestDto);

        StringBuilder csv = new StringBuilder();

        bool isMonthly = csvExportFilterRequestDto.ReportType == ReportType.Monthly;

        // Header
        csv.AppendLine(isMonthly
            ? "\"Username\",\"Month\",\"Category\",\"Amount\",\"Note\""
            : "\"Username\",\"Date\",\"Category\",\"Amount\",\"Note\"");

        Dictionary<string, decimal> categoryTotals = new Dictionary<string, decimal>();
        decimal totalExpense = 0;

        foreach (var exp in expenses)
        {
            string username = Sanitize(exp.User?.Username ?? "Unknown");
            string category = Sanitize(exp.Category?.Name ?? "Uncategorized");
            string note = Sanitize(exp.Note ?? "");

            string dateDisplay = isMonthly
                ? exp.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                : exp.ExpenseDate.ToString("yyyy-MM-dd");

            csv.AppendLine($"\"{username}\",\"{dateDisplay}\",\"{category}\",\"{exp.Amount}\",\"{note}\"");

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += exp.Amount;
            else
                categoryTotals[category] = exp.Amount;

            totalExpense += exp.Amount;
        }

        // Section separator
        csv.AppendLine();
        csv.AppendLine("\"--- Category Totals ---\"");
        csv.AppendLine("\"Category\",\"Total Amount\"");
        foreach (var entry in categoryTotals)
            csv.AppendLine($"\"{entry.Key}\",\"{entry.Value}\"");

        // Grand total
        csv.AppendLine();
        csv.AppendLine("\"Total Expense:\",\"" + totalExpense + "\"");

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));

        ExpenseReport expenseReport = new ExpenseReport
        {
            UserId = userId ?? 0,
        };

        _expenseReportRepository.AddAsync(expenseReport);
        _expenseReportRepository.SaveChangesAsync();

        return stream;
    }

    // Helper to escape values for CSV
    private string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        return value.Replace("\"", "\"\""); // Escape double quotes
    }



}
