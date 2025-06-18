using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;

namespace ExpenseTracker.Repository.Implementation;

public class ExpenseReportRepository : Repository<ExpenseReport>, IExpenseReportRepository
{
    private readonly ExpenseTrackerContext _context;
    public ExpenseReportRepository(ExpenseTrackerContext context) : base(context)
    {
        _context = context;
    }

}
