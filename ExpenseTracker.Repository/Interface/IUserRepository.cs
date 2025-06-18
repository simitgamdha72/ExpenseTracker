using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Dto;
namespace ExpenseTracker.Repository.Interface;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailOrUsernameExistsAsync(string email, string username);
    Task<UserProfileResponseDto?> GetByIdAsync(int? userId);
}
