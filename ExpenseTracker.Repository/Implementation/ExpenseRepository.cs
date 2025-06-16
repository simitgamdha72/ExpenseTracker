using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Repository.Implementation;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    private readonly ExpenseTrackerContext _context;

    public ExpenseRepository(ExpenseTrackerContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Expense>> GetAllExpense()
    {
        return await _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .ToListAsync();
    }

    public IEnumerable<Expense> GetFilteredExpenses(CsvExportFilterDto filter)
    {
        var expenses = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Username))
            expenses = expenses.Where(e => e.User!.Username == filter.Username);

        if (!string.IsNullOrEmpty(filter.Category))
            expenses = expenses.Where(e => e.Category!.Name == filter.Category);

        if (filter.ReportType == "daily" && filter.StartDate.HasValue && filter.EndDate.HasValue)
            expenses = expenses.Where(e => e.ExpenseDate >= filter.StartDate && e.ExpenseDate <= filter.EndDate);

        if (filter.ReportType == "monthly")
        {
            DateOnly from, to;
            if (filter.RangeType == "lastMonth")
            {
                var date = DateTime.Today.AddMonths(-1);
                from = new DateOnly(date.Year, date.Month, 1);
                to = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            }
            else if (filter.RangeType == "last3Months")
            {
                var start = DateTime.Today.AddMonths(-3);
                from = new DateOnly(start.Year, start.Month, 1);
                var end = DateTime.Today;
                to = new DateOnly(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
            }
            else // custom
            {
                if (filter.StartMonth.HasValue && filter.StartYear.HasValue && filter.EndMonth.HasValue && filter.EndYear.HasValue)
                {
                    from = new DateOnly(filter.StartYear.Value, filter.StartMonth.Value, 1);
                    to = new DateOnly(filter.EndYear.Value, filter.EndMonth.Value, DateTime.DaysInMonth(filter.EndYear.Value, filter.EndMonth.Value));
                }
                else
                {
                    from = DateOnly.MinValue;
                    to = DateOnly.MaxValue;
                }
            }

            expenses = expenses.Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to);
        }

        return expenses.ToList();
    }

    public IEnumerable<Expense> GetFilteredUserExpenses(int userId, UserCsvExportFilterDto filter)
    {
        var expenses = _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .AsQueryable();

        if (filter.ReportType == "daily" && filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            expenses = expenses.Where(e => e.ExpenseDate >= filter.StartDate && e.ExpenseDate <= filter.EndDate);
        }
        else if (filter.ReportType == "monthly")
        {
            DateOnly from, to;
            if (filter.RangeType == "lastMonth")
            {
                var d = DateTime.Today.AddMonths(-1);
                from = new DateOnly(d.Year, d.Month, 1);
                to = new DateOnly(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
            }
            else if (filter.RangeType == "last3Months")
            {
                var s = DateTime.Today.AddMonths(-3);
                from = new DateOnly(s.Year, s.Month, 1);
                var e = DateTime.Today;
                to = new DateOnly(e.Year, e.Month, DateTime.DaysInMonth(e.Year, e.Month));
            }
            else // custom
            {
                from = new DateOnly(filter.StartYear!.Value, filter.StartMonth!.Value, 1);
                to = new DateOnly(filter.EndYear!.Value, filter.EndMonth!.Value, DateTime.DaysInMonth(filter.EndYear.Value, filter.EndMonth.Value));
            }

            expenses = expenses.Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to);
        }

        return expenses.ToList();
    }



}
