using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Service.Interface;

public interface IAuthService
{
    Task<Response<object?>> RegisterUserAsync(RegisterRequestDto registerRequestDto);
    Task<Response<object?>> LoginUserAsync(LoginRequestDto loginRequestDto);
}
