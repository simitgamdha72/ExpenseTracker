using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Repository.Interface;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetAllExpense();

    IEnumerable<Expense> GetFilteredExpenses(CsvExportFilterRequestDto csvExportFilterRequestDto);

    IEnumerable<Expense> GetFilteredUserExpenses(int userId, UserCsvExportFilterRequestDto userCsvExportFilterRequestDto);
}
