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

    public async Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(int? userId)
    {
        IEnumerable<Expense> expenses = await _expenseRepository.GetAllAsync();

        List<ExpenseDto> expenseDtos = new List<ExpenseDto>();

        foreach (var e in expenses.Where(e => e.UserId == userId))
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

        return expenseDtos;
    }

    public async Task<ExpenseDto?> GetExpenseByIdAsync(int id, int? userId)
    {
        Expense? expense = await _expenseRepository.GetByIdAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return null;
        }

        ExpenseCategory? expenseCategory = await _expenseCategoryRepository.GetByIdAsync(expense.CategoryId ?? 0);
        return new ExpenseDto
        {
            Id = expense.Id,
            Category = expenseCategory!.Name,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Note = expense.Note
        };
    }

    public async Task<(bool Success, string Message)> CreateExpenseAsync(int? userId, ExpenseDto expenseDto)
    {
        if (!await _expenseCategoryRepository.ExistsByNameAsync(expenseDto.Category ?? ""))
        {
            return (false, ErrorMessages.InvalidCategory);
        }

        ExpenseCategory? Category = await _expenseCategoryRepository.GetCategoryByNameAsync(expenseDto.Category ?? "");

        Expense expense = new Expense
        {
            UserId = userId,
            CategoryId = Category.Id,
            Amount = expenseDto.Amount,
            ExpenseDate = expenseDto.ExpenseDate,
            Note = expenseDto.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();
        return (true, SuccessMessages.ExpenseCreated);
    }

    public async Task<(bool Success, string Message)> UpdateExpenseAsync(int id, int? userId, ExpenseDto expenseDto)
    {
        Expense? expense = await _expenseRepository.GetByIdAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return (false, ErrorMessages.ExpenseNotFound);
        }

        if (!await _expenseCategoryRepository.ExistsByNameAsync(expenseDto.Category ?? ""))
        {
            return (false, ErrorMessages.InvalidCategory);
        }

        ExpenseCategory? Category = await _expenseCategoryRepository.GetCategoryByNameAsync(expenseDto.Category ?? "");

        expense.CategoryId = Category.Id;
        expense.Amount = expenseDto.Amount;
        expense.ExpenseDate = expenseDto.ExpenseDate;
        expense.Note = expenseDto.Note;
        expense.UpdatedAt = DateTime.UtcNow;

        _expenseRepository.Update(expense);
        await _expenseRepository.SaveChangesAsync();

        return (true, SuccessMessages.ExpenseUpdated);
    }

    public async Task<(bool Success, string Message)> DeleteExpenseAsync(int id, int? userId)
    {
        Expense? expense = await _expenseRepository.GetByIdAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return (false, ErrorMessages.ExpenseNotFound);
        }

        _expenseRepository.Delete(expense);
        await _expenseRepository.SaveChangesAsync();
        return (true, SuccessMessages.ExpenseDeleted);
    }

    public async Task<FilteredExpenseReportDto> GetAllUsersExpensesAsync(List<string>? userNames = null)
    {
        IEnumerable<Expense> expenses = await _expenseRepository.GetAllExpense();

        List<ExpenseDetailsDto>? expenseDetailsDto = new List<ExpenseDetailsDto>();
        List<string>? notFoundUsernames = new List<string>();

        if (userNames != null && userNames.Any())
        {
            HashSet<string>? foundUsernames = expenses.Select(e => e.User!.Username).Distinct().ToHashSet();
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

        return new FilteredExpenseReportDto
        {
            Expenses = expenseDetailsDto,
            NotFoundUsernames = notFoundUsernames
        };
    }
}

