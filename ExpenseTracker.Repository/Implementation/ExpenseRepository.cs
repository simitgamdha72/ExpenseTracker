using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
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

    public IEnumerable<Expense> GetFilteredExpenses(CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        IQueryable<Expense>? expenses = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(csvExportFilterRequestDto.Username))
            expenses = expenses.Where(e => e.User!.Username.ToLower().Contains(csvExportFilterRequestDto.Username));

        if (!string.IsNullOrEmpty(csvExportFilterRequestDto.Category))
            expenses = expenses.Where(e => e.Category!.Name.ToLower().Contains(csvExportFilterRequestDto.Category));


        if (csvExportFilterRequestDto.ReportType == ReportType.Daily && csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue)
            expenses = expenses.Where(e => e.ExpenseDate >= csvExportFilterRequestDto.StartDate && e.ExpenseDate <= csvExportFilterRequestDto.EndDate);

        if (csvExportFilterRequestDto.ReportType == ReportType.Monthly)
        {
            DateOnly from, to;
            if (csvExportFilterRequestDto.RangeType == RangeType.LastMonth)
            {
                DateTime date = DateTime.Today.AddMonths(-1);
                from = new DateOnly(date.Year, date.Month, 1);
                to = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            }
            else if (csvExportFilterRequestDto.RangeType == RangeType.Last3Months)
            {
                DateTime start = DateTime.Today.AddMonths(-3);
                from = new DateOnly(start.Year, start.Month, 1);
                DateTime end = DateTime.Today;
                to = new DateOnly(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
            }
            else // custom
            {
                if (csvExportFilterRequestDto.StartMonth.HasValue && csvExportFilterRequestDto.StartYear.HasValue && csvExportFilterRequestDto.EndMonth.HasValue && csvExportFilterRequestDto.EndYear.HasValue)
                {
                    from = new DateOnly(csvExportFilterRequestDto.StartYear.Value, csvExportFilterRequestDto.StartMonth.Value, 1);

                    DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value,
                        DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));

                    if (csvExportFilterRequestDto.EndYear.Value == DateTime.Today.Year &&
                        csvExportFilterRequestDto.EndMonth.Value == DateTime.Today.Month)
                    {
                        to = DateOnly.FromDateTime(DateTime.Today);
                    }
                    else
                    {
                        to = end;
                    }
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

    public IEnumerable<Expense> GetFilteredUserExpenses(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto)
    {
        IQueryable<Expense>? expenses = _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .AsQueryable();

        if (userCsvExportFilterRequestDto.ReportType == ReportType.Daily && userCsvExportFilterRequestDto.StartDate.HasValue && userCsvExportFilterRequestDto.EndDate.HasValue)
        {
            expenses = expenses.Where(e => e.ExpenseDate >= userCsvExportFilterRequestDto.StartDate && e.ExpenseDate <= userCsvExportFilterRequestDto.EndDate);
        }
        else if (userCsvExportFilterRequestDto.ReportType == ReportType.Monthly)
        {
            DateOnly from, to;
            if (userCsvExportFilterRequestDto.RangeType == RangeType.LastMonth)
            {
                DateTime d = DateTime.Today.AddMonths(-1);
                from = new DateOnly(d.Year, d.Month, 1);
                to = new DateOnly(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
            }
            else if (userCsvExportFilterRequestDto.RangeType == RangeType.Last3Months)
            {
                DateTime s = DateTime.Today.AddMonths(-3);
                from = new DateOnly(s.Year, s.Month, 1);
                DateTime e = DateTime.Today;
                to = new DateOnly(e.Year, e.Month, DateTime.DaysInMonth(e.Year, e.Month));
            }
            else // custom
            {
                if (userCsvExportFilterRequestDto.StartMonth.HasValue && userCsvExportFilterRequestDto.StartYear.HasValue && userCsvExportFilterRequestDto.EndMonth.HasValue && userCsvExportFilterRequestDto.EndYear.HasValue)
                {
                    from = new DateOnly(userCsvExportFilterRequestDto.StartYear.Value, userCsvExportFilterRequestDto.StartMonth.Value, 1);

                    DateOnly end = new DateOnly(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value,
                        DateTime.DaysInMonth(userCsvExportFilterRequestDto.EndYear.Value, userCsvExportFilterRequestDto.EndMonth.Value));

                    if (userCsvExportFilterRequestDto.EndYear.Value == DateTime.Today.Year &&
                        userCsvExportFilterRequestDto.EndMonth.Value == DateTime.Today.Month)
                    {
                        to = DateOnly.FromDateTime(DateTime.Today);
                    }
                    else
                    {
                        to = end;
                    }
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



}
