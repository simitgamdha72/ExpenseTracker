using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;

namespace ExpenseTracker.Service.Implementation;

public class ExpenseCategoriesService : IExpenseCategoriesService
{
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;

    public ExpenseCategoriesService(IExpenseCategoryRepository expenseCategoryRepository)
    {
        _expenseCategoryRepository = expenseCategoryRepository;
    }

    public async Task<IEnumerable<ExpenseCategoryDto>> GetCategoriesAsync()
    {
        IEnumerable<ExpenseCategory> expenseCategorylist = await _expenseCategoryRepository.GetAllAsync();

        return expenseCategorylist.Select(c => new ExpenseCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });
    }

    public async Task<ExpenseCategoryDto?> GetCategoryAsync(int id)
    {
        ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return null;
        }

        return new ExpenseCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<(bool, string, ExpenseCategory?)> CreateCategoryAsync(ExpenseCategoryDto expenseCategoryDto)
    {
        if (await _expenseCategoryRepository.ExistsByNameAsync(expenseCategoryDto.Name))
        {
            return (false, "Category already exists", null);
        }

        ExpenseCategory category = new ExpenseCategory
        {
            Name = expenseCategoryDto.Name,
            Description = expenseCategoryDto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _expenseCategoryRepository.AddAsync(category);
        await _expenseCategoryRepository.SaveChangesAsync();
        return (true, "Created", category);
    }

    public async Task<(bool, string, ExpenseCategory?)> UpdateCategoryAsync(int id, ExpenseCategoryDto expenseCategoryDto)
    {
        ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return (false, "Not found", null);
        }

        if (await _expenseCategoryRepository.ExistsByNameExceptIdAsync(expenseCategoryDto.Name, id))
        {
            throw new Exception("Exist");
        }

        category.Name = expenseCategoryDto.Name;
        category.Description = expenseCategoryDto.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _expenseCategoryRepository.SaveChangesAsync();
        return (true, "Updated", category);
    }

    public async Task<(bool, string)> DeleteCategoryAsync(int id)
    {
        ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return (false, "Category not found");
        }

        _expenseCategoryRepository.Delete(category);
        await _expenseCategoryRepository.SaveChangesAsync();
        return (true, "Category Deleted Successfully!");
    }
}
