using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto loginDto);
    Task<string> RegisterAsync(RegisterDto registerDto);
}
