using System.Net;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
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

    public async Task<Response<object>> GetCategoriesWithResponseAsync()
    {
        try
        {
            IEnumerable<ExpenseCategory>? categories = await _expenseCategoryRepository.GetAllAsync();

            IEnumerable<ExpenseCategoryDto>? dtoList = categories.Select(c => new ExpenseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });

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
            ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
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
            bool exists = await _expenseCategoryRepository.ExistsByNameAsync(expenseCategoryDto.Name);
            if (exists)
            {
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
            ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
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
            ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
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
