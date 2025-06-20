using System.Net;
using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Service.Implementation;

public class ExpensesService : IExpensesService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;
    private readonly ILogger<ExpensesService> _logger;

    public ExpensesService(IExpenseRepository expenseRepository, IExpenseCategoryRepository expenseCategoryRepository, ILogger<ExpensesService> logger)
    {
        _expenseRepository = expenseRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
        _logger = logger;
    }

    public async Task<Response<object>> GetExpensesWithResponseAsync(ClaimsPrincipal user)
    {
        try
        {
            _logger.LogInformation("Fetching expenses for the authenticated user.");

            if (!user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt: user not authenticated.");

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
                _logger.LogWarning("Invalid user ID extracted from claims.");

                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            _logger.LogInformation("User ID {UserId} authenticated. Fetching expenses...", userId);

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

            _logger.LogInformation("Fetched {Count} expenses for user ID {UserId}.", expenseDtos.Count, userId);

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
            _logger.LogError(ex, "An error occurred while fetching expenses for the user.");

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
            _logger.LogInformation("Attempting to fetch expense with ID: {ExpenseId}", id);

            if (!user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized request for expense ID {ExpenseId}: user not authenticated.", id);

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
                _logger.LogWarning("Invalid user ID extracted while requesting expense ID {ExpenseId}.", id);

                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            _logger.LogInformation("Fetching expense ID {ExpenseId} for user ID {UserId}.", id, userId);

            Expense? expense = await _expenseRepository.GetByIdAsync(id);

            if (expense == null || expense.UserId != userId)
            {
                _logger.LogWarning("Expense not found or doesn't belong to user. Expense ID: {ExpenseId}, User ID: {UserId}", id, userId);

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

            _logger.LogInformation("Expense ID {ExpenseId} successfully fetched for user ID {UserId}.", id, userId);

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
            _logger.LogError(ex, "An error occurred while fetching expense with ID: {ExpenseId}", id);

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
            _logger.LogInformation("Creating expense for category: {Category}", expenseDto.Category);

            if (!user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized request to create expense. User not authenticated.");

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
                _logger.LogWarning("Invalid user ID while creating expense.");

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
                _logger.LogWarning("Attempted to create expense with invalid category: {Category}", expenseDto.Category);

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

            _logger.LogInformation("Expense created successfully. ID: {ExpenseId}, User ID: {UserId}", expense.Id, userId);

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
            _logger.LogError(ex, "An error occurred while creating an expense.");

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
            _logger.LogInformation("Attempting to update expense ID: {ExpenseId}", id);

            if (!user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized update attempt for expense ID {ExpenseId}. User not authenticated.", id);

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
                _logger.LogWarning("Invalid user ID while updating expense ID {ExpenseId}.", id);

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
                _logger.LogWarning("Expense not found or does not belong to the user. Expense ID: {ExpenseId}, User ID: {UserId}", id, userId);

                return new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.ExpenseNotFound }
                };
            }

            if (!await _expenseCategoryRepository.ExistsByNameAsync(expenseDto.Category ?? ""))
            {
                _logger.LogWarning("Invalid category '{Category}' for update of expense ID: {ExpenseId}", expenseDto.Category, id);

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

            _logger.LogInformation("Expense ID {ExpenseId} updated successfully by user ID {UserId}.", expense.Id, userId);

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
            _logger.LogError(ex, "An error occurred while updating expense ID: {ExpenseId}", id);

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
            _logger.LogInformation("Attempting to delete expense with ID: {ExpenseId}", id);

            if (!user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized delete attempt for expense ID {ExpenseId}. User not authenticated.", id);

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
                _logger.LogWarning("Invalid user ID during delete attempt. Expense ID: {ExpenseId}", id);

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
                _logger.LogWarning("Expense not found or does not belong to user. Expense ID: {ExpenseId}, User ID: {UserId}", id, userId);

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

            _logger.LogInformation("Expense ID {ExpenseId} deleted successfully by user ID {UserId}.", id, userId);

            return new Response<object>
            {
                Message = SuccessMessages.ExpenseDeleted,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting expense ID: {ExpenseId}", id);

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
            _logger.LogInformation("Fetching all users' expenses. Filtering by usernames: {Usernames}", userNames != null ? string.Join(", ", userNames) : "None");


            IEnumerable<Expense> expenses = await _expenseRepository.GetAllExpense();

            List<ExpenseDetailsDto> expenseDetailsDto = new();
            List<string> notFoundUsernames = new();

            if (userNames != null && userNames.Any())
            {
                HashSet<string> foundUsernames = expenses.Select(e => e.User!.Username).Distinct().ToHashSet();
                notFoundUsernames = userNames.Where(un => !foundUsernames.Contains(un)).ToList();
                expenses = expenses.Where(e => userNames.Contains(e.User!.Username));

                _logger.LogInformation("Usernames not found: {NotFound}", string.Join(", ", notFoundUsernames));

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

            _logger.LogInformation("Fetched {Count} expenses. {MissingCount} usernames not found.", expenseDetailsDto.Count, notFoundUsernames.Count);

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
            _logger.LogError(ex, "An error occurred while fetching expenses for users.");

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

