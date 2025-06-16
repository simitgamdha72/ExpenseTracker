using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IExpensesService
{
    Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(int? userId);
    Task<ExpenseDto?> GetExpenseByIdAsync(int id, int? userId);
    Task<(bool Success, string Message)> CreateExpenseAsync(int? userId, ExpenseDto dto);
    Task<(bool Success, string Message)> UpdateExpenseAsync(int id, int? userId, ExpenseDto dto);
    Task<(bool Success, string Message)> DeleteExpenseAsync(int id, int? userId);
    Task<FilteredExpenseReportDto> GetAllUsersExpensesAsync(List<string>? userNames = null);
}
