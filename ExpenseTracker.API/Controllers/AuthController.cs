using System.Net;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Service.Validations;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost("register")]
    [Trim]
    public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
    {
        try
        {

            if (registerRequestDto.RoleId != 1 && registerRequestDto.RoleId != 2)
            {
                return BadRequest("Invalid role. Role must be 1 or 2.");
            }

            string message = await _authService.RegisterAsync(registerRequestDto);

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.RegistrationFailed);

            if (ex.Message == ErrorMessages.EmailOrUsernameExists)
            {
                return Conflict(ErrorMessages.EmailOrUsernameExists);
            }
            else
            {
                return BadRequest(ex.Message);
            }

        }
    }

    [HttpPost("login")]
    [Trim]
    public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
    {
        try
        {

            string? token = await _authService.LoginAsync(loginRequestDto);

            if (token == null)
            {
                return Unauthorized(ErrorMessages.InvalidCredentials);
            }

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.LoginFailed);
            return StatusCode(500, ErrorMessages.LoginFailed);
        }
    }

}
