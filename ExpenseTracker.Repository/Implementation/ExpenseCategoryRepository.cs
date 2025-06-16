using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Repository.Implementation;

public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
{
    private readonly ExpenseTrackerContext _context;

    public ExpenseCategoryRepository(ExpenseTrackerContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.ExpenseCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameExceptIdAsync(string name, int id)
    {
        return await _context.ExpenseCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != id);
    }

    public async Task<ExpenseCategory> GetCategoryByNameAsync(string categoryName)
    {
        ExpenseCategory? expenseCategory = await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

        if (expenseCategory == null)
        {
            return new ExpenseCategory();
        }

        return expenseCategory;
    }

}
