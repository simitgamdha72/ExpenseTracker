using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Service.Interface;

public interface IUserService
{
    Task<UserProfileResponseDto?> GetUserByIdAsync(int? userId);
}
