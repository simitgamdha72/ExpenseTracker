using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Service.Interface;

public interface IExpenseCategoriesService
{
    Task<IEnumerable<ExpenseCategoryDto>> GetCategoriesAsync();
    Task<ExpenseCategoryDto?> GetCategoryAsync(int id);
    Task<(bool Success, string Message, ExpenseCategory? Category)> CreateCategoryAsync(ExpenseCategoryDto dto);
    Task<(bool Success, string Message, ExpenseCategory? Category)> UpdateCategoryAsync(int id, ExpenseCategoryDto dto);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
}
