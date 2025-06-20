using System.Security.Claims;
using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IExpensesService
{
    Task<Response<object>> GetExpensesWithResponseAsync(ClaimsPrincipal user);
    Task<Response<object>> GetExpenseResponseByIdAsync(int id, ClaimsPrincipal user);
    Task<Response<object>> CreateExpenseResponseAsync(ExpenseDto expenseDto, ClaimsPrincipal user);
    Task<Response<object>> UpdateExpenseResponseAsync(int id, ExpenseDto expenseDto, ClaimsPrincipal user);
    Task<Response<object>> DeleteExpenseResponseAsync(int id, ClaimsPrincipal user);
    Task<Response<FilteredExpenseReportDto>> GetAllUsersExpensesResponseAsync(List<string>? userNames = null);

}
