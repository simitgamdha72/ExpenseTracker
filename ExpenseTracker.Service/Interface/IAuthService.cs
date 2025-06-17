using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginRequestDto loginRequestDto);
    Task<string> RegisterAsync(RegisterRequestDto registerRequestDto);
}
