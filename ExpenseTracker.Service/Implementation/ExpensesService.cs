using System.Net;
using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;

namespace ExpenseTracker.Service.Implementation;

public class ExpensesService : IExpensesService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;

    public ExpensesService(IExpenseRepository expenseRepository, IExpenseCategoryRepository expenseCategoryRepository)
    {
        _expenseRepository = expenseRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
    }

    public async Task<Response<object>> GetExpensesWithResponseAsync(ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            IEnumerable<Expense> expenses = await _expenseRepository.GetAllAsync();
            IEnumerable<Expense>? filtered = expenses.Where(e => e.UserId == userId);

            List<ExpenseDto> expenseDtos = new List<ExpenseDto>();
            foreach (var e in filtered)
            {
                ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(e.CategoryId ?? 0);

                expenseDtos.Add(new ExpenseDto
                {
                    Id = e.Id,
                    Category = category?.Name,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    Note = e.Note
                });
            }

            return new Response<object>
            {
                Message = SuccessMessages.ExpensesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = expenseDtos
            };
        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Message = ErrorMessages.GetExpensesFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> GetExpenseResponseByIdAsync(int id, ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            Expense? expense = await _expenseRepository.GetByIdAsync(id);

            if (expense == null || expense.UserId != userId)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.ExpenseNotFound,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null
                };
            }

            ExpenseCategory? expenseCategory = await _expenseCategoryRepository.GetByIdAsync(expense.CategoryId ?? 0);

            ExpenseDto dto = new ExpenseDto
            {
                Id = expense.Id,
                Category = expenseCategory?.Name ?? "Uncategorized",
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Note = expense.Note
            };

            return new Response<object>
            {
                Message = SuccessMessages.ExpensesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = dto
            };
        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Message = ErrorMessages.GetExpensesFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> CreateExpenseResponseAsync(ExpenseDto expenseDto, ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            if (!await _expenseCategoryRepository.ExistsByNameAsync(expenseDto.Category ?? ""))
            {
                return new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.InvalidCategory }
                };
            }

            ExpenseCategory category = await _expenseCategoryRepository.GetCategoryByNameAsync(expenseDto.Category ?? "");

            Expense expense = new Expense
            {
                UserId = userId,
                CategoryId = category.Id,
                Amount = expenseDto.Amount,
                ExpenseDate = expenseDto.ExpenseDate,
                Note = expenseDto.Note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _expenseRepository.AddAsync(expense);
            await _expenseRepository.SaveChangesAsync();

            ExpenseDto createdDto = new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Category = category.Name,
                ExpenseDate = expense.ExpenseDate,
                Note = expense.Note
            };

            return new Response<object>
            {
                Message = SuccessMessages.ExpenseCreated,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = createdDto
            };
        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Message = ErrorMessages.CreateExpenseFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> UpdateExpenseResponseAsync(int id, ExpenseDto expenseDto, ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            Expense? expense = await _expenseRepository.GetByIdAsync(id);

            if (expense == null || expense.UserId != userId)
            {
                return new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.ExpenseNotFound }
                };
            }

            if (!await _expenseCategoryRepository.ExistsByNameAsync(expenseDto.Category ?? ""))
            {
                return new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.InvalidCategory }
                };
            }

            ExpenseCategory? category = await _expenseCategoryRepository.GetCategoryByNameAsync(expenseDto.Category ?? "");

            expense.CategoryId = category.Id;
            expense.Amount = expenseDto.Amount;
            expense.ExpenseDate = expenseDto.ExpenseDate;
            expense.Note = expenseDto.Note;
            expense.UpdatedAt = DateTime.UtcNow;

            _expenseRepository.Update(expense);
            await _expenseRepository.SaveChangesAsync();

            ExpenseDto? updatedExpense = new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Category = category.Name,
                ExpenseDate = expense.ExpenseDate,
                Note = expense.Note
            };

            return new Response<object>
            {
                Message = SuccessMessages.ExpenseUpdated,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = updatedExpense
            };
        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Message = ErrorMessages.UpdateExpenseFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object>> DeleteExpenseResponseAsync(int id, ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId == 0)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            Expense? expense = await _expenseRepository.GetByIdAsync(id);

            if (expense == null || expense.UserId != userId)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.ExpenseNotFound,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.ExpenseNotFound }
                };
            }

            _expenseRepository.Delete(expense);
            await _expenseRepository.SaveChangesAsync();

            return new Response<object>
            {
                Message = SuccessMessages.ExpenseDeleted,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Message = ErrorMessages.DeleteExpenseFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<FilteredExpenseReportDto>> GetAllUsersExpensesResponseAsync(List<string>? userNames = null)
    {
        try
        {
            IEnumerable<Expense> expenses = await _expenseRepository.GetAllExpense();

            List<ExpenseDetailsDto> expenseDetailsDto = new();
            List<string> notFoundUsernames = new();

            if (userNames != null && userNames.Any())
            {
                HashSet<string> foundUsernames = expenses.Select(e => e.User!.Username).Distinct().ToHashSet();
                notFoundUsernames = userNames.Where(un => !foundUsernames.Contains(un)).ToList();
                expenses = expenses.Where(e => userNames.Contains(e.User!.Username));
            }

            foreach (var e in expenses)
            {
                ExpenseCategory? category = await _expenseCategoryRepository.GetByIdAsync(e.CategoryId ?? 0);

                expenseDetailsDto.Add(new ExpenseDetailsDto
                {
                    expenseDto = new ExpenseDto
                    {
                        Id = e.Id,
                        Category = category?.Name,
                        Amount = e.Amount,
                        ExpenseDate = e.ExpenseDate,
                        Note = e.Note
                    },
                    UserName = e.User!.Username
                });
            }

            FilteredExpenseReportDto data = new()
            {
                Expenses = expenseDetailsDto,
                NotFoundUsernames = notFoundUsernames
            };

            return new Response<FilteredExpenseReportDto>
            {
                Message = SuccessMessages.ExpensesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new Response<FilteredExpenseReportDto>
            {
                Message = ErrorMessages.GetExpensesFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Errors = new[] { ex.Message }
            };
        }
    }

}

