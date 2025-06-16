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
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string message = await _authService.RegisterAsync(registerDto);

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
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? token = await _authService.LoginAsync(loginDto);

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
