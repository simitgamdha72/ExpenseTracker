using System.Net;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Service.Implementation;

public class ExpenseCategoriesService : IExpenseCategoriesService
{
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;
    private readonly ILogger<ExpenseCategoriesService> _logger;

    public ExpenseCategoriesService(IExpenseCategoryRepository expenseCategoryRepository, ILogger<ExpenseCategoriesService> logger)
    {
        _expenseCategoryRepository = expenseCategoryRepository;
        _logger = logger;
    }

    public async Task<Response<object>> GetCategoriesWithResponseAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all expense categories.");

            IEnumerable<ExpenseCategory>? categories = await _expenseCategoryRepository.GetAllAsync();

            IEnumerable<ExpenseCategoryDto>? dtoList = categories.Select(c => new ExpenseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });

            _logger.LogInformation("Fetched {Count} expense categories successfully.", dtoList.Count());

            return new Response<object>
            {
                Message = SuccessMessages.CategoriesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = dtoList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching expense categories.");

            return new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> GetCategoryWithResponseAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching expense category with ID: {CategoryId}", id);

            ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Expense category not found for ID: {CategoryId}", id);

                return new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.NotFound }
                };
            }

            ExpenseCategoryDto? dto = new ExpenseCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            _logger.LogInformation("Successfully fetched category: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

            return new Response<object>
            {
                Message = SuccessMessages.CategoryFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = dto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching category with ID: {CategoryId}", id);

            return new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> CreateCategoryWithResponseAsync(ExpenseCategoryDto expenseCategoryDto)
    {
        try
        {
            _logger.LogInformation("Creating expense category with name: {CategoryName}", expenseCategoryDto.Name);

            bool exists = await _expenseCategoryRepository.ExistsByNameAsync(expenseCategoryDto.Name);
            if (exists)
            {
                _logger.LogWarning("Category creation failed: Category name already exists - {CategoryName}", expenseCategoryDto.Name);

                return new Response<object>
                {
                    Message = ErrorMessages.CategoryNameExists,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Errors = new[] { ErrorMessages.CategoryNameExists },
                    Data = null
                };
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

            _logger.LogInformation("Expense category created successfully with ID: {CategoryId}", category.Id);

            return new Response<object>
            {
                Message = SuccessMessages.Created,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.Created,
                Data = category
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while creating expense category: {CategoryName}", expenseCategoryDto.Name);

            return new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> UpdateCategoryWithResponseAsync(int id, ExpenseCategoryDto expenseCategoryDto)
    {
        try
        {
            _logger.LogInformation("Attempting to update expense category with ID: {CategoryId}", id);

            ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Category not found for update. ID: {CategoryId}", id);

                return new Response<object>
                {
                    Message = ErrorMessages.CategoryNotFound,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.NotFound }
                };
            }

            bool nameExists = await _expenseCategoryRepository.ExistsByNameExceptIdAsync(expenseCategoryDto.Name, id);
            if (nameExists)
            {
                _logger.LogWarning("Category name already exists for another category. Name: {CategoryName}, ID: {CategoryId}",
                   expenseCategoryDto.Name, id);

                return new Response<object>
                {
                    Message = ErrorMessages.CategoryNameExists,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Data = null,
                    Errors = new[] { ErrorMessages.CategoryNameExists }
                };
            }

            category.Name = expenseCategoryDto.Name;
            category.Description = expenseCategoryDto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _expenseCategoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category updated successfully. ID: {CategoryId}", id);

            return new Response<object>
            {
                Message = SuccessMessages.Updated,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = category
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating category with ID: {CategoryId}", id);

            return new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> DeleteCategoryWithResponseAsync(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete expense category with ID: {CategoryId}", id);

            ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Category not found for deletion. ID: {CategoryId}", id);

                return new Response<object>
                {
                    Message = ErrorMessages.CategoryNotFound,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.NotFound }
                };
            }

            _expenseCategoryRepository.Delete(category);
            await _expenseCategoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category deleted successfully. ID: {CategoryId}", id);

            return new Response<object>
            {
                Message = SuccessMessages.CategoryDeleted,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting category with ID: {CategoryId}", id);

            return new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

}
