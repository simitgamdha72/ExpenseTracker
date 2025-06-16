using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Repository.Interface;

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByNameExceptIdAsync(string name, int id);
    Task<ExpenseCategory> GetCategoryByNameAsync(string categoryName);
}
