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

    public ReportService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public MemoryStream ExportUserExpensesToCsv(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredUserExpenses(userId, userCsvExportFilterRequestDto);

        StringBuilder? csv = new StringBuilder();
        csv.AppendLine("Date,Category,Amount,Note");

        Dictionary<string, decimal>? categoryTotals = new Dictionary<string, decimal>();
        decimal total = 0;

        foreach (var e in expenses)
        {
            string category = e.Category?.Name ?? "Uncategorized";
            csv.AppendLine($"{e.ExpenseDate:yyyy-MM-dd},{category},{e.Amount},{e.Note}");

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += e.Amount;
            else
                categoryTotals[category] = e.Amount;

            total += e.Amount;
        }

        csv.AppendLine();
        csv.AppendLine("Category Totals:");
        csv.AppendLine("Category,TotalAmount");
        foreach (var c in categoryTotals)
            csv.AppendLine($"{c.Key},{c.Value}");

        csv.AppendLine();
        csv.AppendLine($"Total Expense:,{total}");

        return new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    public object GetUserExpenseSummary(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredUserExpenses(userId, userCsvExportFilterRequestDto);
        Dictionary<string, decimal>? categoryTotals = new Dictionary<string, decimal>();
        decimal total = 0;

        bool isMonthly = userCsvExportFilterRequestDto.ReportType == ReportType.Monthly;
        List<object> expenseList = new();

        foreach (var e in expenses)
        {
            string category = e.Category?.Name ?? "Uncategorized";
            string dateDisplay = isMonthly
                ? e.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                : e.ExpenseDate.ToString("yyyy-MM-dd");

            expenseList.Add(new
            {
                Date = dateDisplay,
                Category = category,
                Amount = e.Amount,
                Note = e.Note
            });

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += e.Amount;
            else
                categoryTotals[category] = e.Amount;

            total += e.Amount;
        }

        return new
        {
            Expenses = expenseList,
            CategoryTotals = categoryTotals.Select(ct => new { Category = ct.Key, TotalAmount = ct.Value }),
            TotalExpense = total
        };
    }

}
