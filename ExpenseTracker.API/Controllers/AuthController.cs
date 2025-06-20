using System.Net;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Service.Validations;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [Trim]
    public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
    {
        Response<object?>? result = await _authService.RegisterUserAsync(registerRequestDto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    [Trim]
    public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
    {
        Response<object?>? result = await _authService.LoginUserAsync(loginRequestDto);
        return StatusCode(result.StatusCode, result);
    }

}
