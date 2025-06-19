using System.Text;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;

namespace ExpenseTracker.Service.Implementation;

public class ReportService : IReportService
{

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseReportRepository _expenseReportRepository;

    public ReportService(IExpenseRepository expenseRepository, IExpenseReportRepository expenseReportRepository)
    {
        _expenseRepository = expenseRepository;
        _expenseReportRepository = expenseReportRepository;
    }

    public MemoryStream ExportUserExpensesToCsv(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredUserExpenses(userId, userCsvExportFilterRequestDto);

        StringBuilder csv = new StringBuilder();

        // Header
        csv.AppendLine("\"Date\",\"Category\",\"Amount\",\"Note\"");

        Dictionary<string, decimal> categoryTotals = new();
        decimal total = 0;

        foreach (var e in expenses)
        {
            string category = SanitizeForCsv(e.Category?.Name ?? "Uncategorized");
            string note = SanitizeForCsv(e.Note ?? "");

            string date = e.ExpenseDate.ToString("yyyy-MM-dd");
            string amount = e.Amount.ToString();

            csv.AppendLine($"\"{date}\",\"{category}\",\"{amount}\",\"{note}\"");

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += e.Amount;
            else
                categoryTotals[category] = e.Amount;

            total += e.Amount;
        }

        // Category totals section
        csv.AppendLine();
        csv.AppendLine("\"--- Category Totals ---\"");
        csv.AppendLine("\"Category\",\"TotalAmount\"");
        foreach (var c in categoryTotals)
            csv.AppendLine($"\"{c.Key}\",\"{c.Value}\"");

        // Grand total
        csv.AppendLine();
        csv.AppendLine("\"Total Expense:\",\"" + total + "\"");

        ExpenseReport expenseReport = new()
        {
            UserId = userId,
        };
        _expenseReportRepository.AddAsync(expenseReport);
        _expenseReportRepository.SaveChangesAsync();

        return new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    private static string SanitizeForCsv(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        // Escape quotes
        string sanitized = input.Replace("\"", "\"\"");
        return sanitized;
    }

    public object GetUserExpenseSummary(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredUserExpenses(userId, userCsvExportFilterRequestDto);

        bool isMonthly = userCsvExportFilterRequestDto.ReportType == ReportType.Monthly;

        var groupedByCategory = expenses
            .GroupBy(e => e.Category?.Name ?? "Uncategorized")
            .Select(group => new
            {
                Category = group.Key,
                TotalAmount = group.Sum(e => e.Amount),
                Expenses = group.Select(e => new
                {
                    Date = isMonthly
                        ? e.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                        : e.ExpenseDate.ToString("yyyy-MM-dd"),
                    Amount = e.Amount,
                    Note = e.Note
                }).ToList()
            }).ToList();

        decimal totalExpense = groupedByCategory.Sum(g => g.TotalAmount);

        return new
        {
            Categories = groupedByCategory,
            TotalExpense = totalExpense
        };
    }


}
