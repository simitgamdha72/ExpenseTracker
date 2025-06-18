using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Service.Interface;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginRequestDto loginRequestDto);
    Task<User> RegisterAsync(RegisterRequestDto registerRequestDto);
}
