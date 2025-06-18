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
                Response<object?> responseError = new Response<object?>
                {
                    Message = ErrorMessages.InvalidRole,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Succeeded = false,
                };
                return BadRequest(responseError);
            }

            User user = await _authService.RegisterAsync(registerRequestDto);

            Response<User> response = new Response<User>
            {
                Message = SuccessMessages.UserRegistered,
                StatusCode = (int)HttpStatusCode.OK,
                Succeeded = true,
                Data = user
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.RegistrationFailed);

            if (ex.Message == ErrorMessages.EmailOrUsernameExists)
            {

                Response<object> response = new Response<object>
                {
                    Message = ErrorMessages.EmailOrUsernameExists,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Data = null,
                    Errors = new[] { ex.Message }
                };

                return BadRequest(response);
            }
            else
            {
                Response<object> response = new Response<object>
                {
                    Message = ErrorMessages.RegistrationFailed,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                    Errors = new[] { ex.Message }
                };

                return BadRequest(response);
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
                Response<object?> responseError = new Response<object?>
                {
                    Message = ErrorMessages.InvalidCredentials,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Succeeded = false,
                };
                return BadRequest(responseError);
            }

            Response<object> response = new Response<object>
            {
                Message = SuccessMessages.LoginSuccessful,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = token
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.LoginFailed);

            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.LoginFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return BadRequest(response);
        }
    }

}
