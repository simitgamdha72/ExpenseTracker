using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Repository.Interface;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailOrUsernameExistsAsync(string email, string username);
}
