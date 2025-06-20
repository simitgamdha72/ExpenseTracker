using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Service.Interface;

public interface IExpenseCategoriesService
{
    Task<Response<object>> GetCategoriesWithResponseAsync();
    Task<Response<object>> GetCategoryWithResponseAsync(int id);
    Task<Response<object>> CreateCategoryWithResponseAsync(ExpenseCategoryDto expenseCategoryDto);
    Task<Response<object>> UpdateCategoryWithResponseAsync(int id, ExpenseCategoryDto expenseCategoryDto);
    Task<Response<object>> DeleteCategoryWithResponseAsync(int id);
}
