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

    public DashboardService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public object GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredExpenses(csvExportFilterRequestDto);

        Dictionary<string, decimal> categoryTotals = new Dictionary<string, decimal>();
        decimal totalExpense = 0;

        List<object> expenseList = new();
        bool isMonthly = csvExportFilterRequestDto.ReportType == ReportType.Monthly;

        foreach (var exp in expenses)
        {
            string category = exp.Category?.Name ?? "Uncategorized";
            string dateDisplay = isMonthly
                ? exp.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                : exp.ExpenseDate.ToString("yyyy-MM-dd");

            expenseList.Add(new
            {
                Username = exp.User?.Username,
                Date = dateDisplay,
                Category = category,
                Amount = exp.Amount,
                Note = exp.Note
            });

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += exp.Amount;
            else
                categoryTotals[category] = exp.Amount;

            totalExpense += exp.Amount;
        }

        return new
        {
            Expenses = expenseList,
            CategoryTotals = categoryTotals.Select(ct => new { Category = ct.Key, TotalAmount = ct.Value }),
            TotalExpense = totalExpense
        };
    }


    public MemoryStream ExportExpensesToCsv(CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredExpenses(csvExportFilterRequestDto);

        StringBuilder? csv = new StringBuilder();

        bool isMonthly = csvExportFilterRequestDto.ReportType == ReportType.Monthly;

        csv.AppendLine(isMonthly ? "Username,Month,Category,Amount,Note" : "Username,Date,Category,Amount,Note");

        Dictionary<string, decimal>? categoryTotals = new Dictionary<string, decimal>();
        decimal totalExpense = 0;

        foreach (var exp in expenses)
        {
            string category = exp.Category?.Name ?? "Uncategorized";
            string dateDisplay = isMonthly
                ? exp.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                : exp.ExpenseDate.ToString("yyyy-MM-dd");

            csv.AppendLine($"{exp.User?.Username},{dateDisplay},{category},{exp.Amount},{exp.Note}");

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += exp.Amount;
            else
                categoryTotals[category] = exp.Amount;

            totalExpense += exp.Amount;
        }

        // Category totals section
        csv.AppendLine();
        csv.AppendLine("Category Totals:");
        csv.AppendLine("Category,TotalAmount");
        foreach (var entry in categoryTotals)
            csv.AppendLine($"{entry.Key},{entry.Value}");

        // Grand total
        csv.AppendLine();
        csv.AppendLine($"Total Expense:,{totalExpense}");

        MemoryStream? stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
        return stream;
    }




}
