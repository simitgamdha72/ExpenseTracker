using System.Net;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Service.Validations;

namespace ExpenseTracker.Service.Implementation;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwt;

    public AuthService(IUserRepository userRepository, JwtService jwt)
    {
        _userRepository = userRepository;
        _jwt = jwt;
    }

    public async Task<Response<object?>> LoginUserAsync(LoginRequestDto loginRequestDto)
    {
        try
        {
            // Check if user exists
            User? user = await _userRepository.GetByEmailAsync(loginRequestDto.Email);

            // Invalid credentials
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.PasswordHash))
            {
                return new Response<object?>
                {
                    Message = ErrorMessages.InvalidCredentials,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Succeeded = false
                };
            }

            // Generate JWT token
            string token = _jwt.GenerateToken(user);

            return new Response<object?>
            {
                Message = SuccessMessages.LoginSuccessful,
                StatusCode = (int)HttpStatusCode.OK,
                Succeeded = true,
                Data = token
            };
        }
        catch (Exception ex)
        {
            return new Response<object?>
            {
                Message = ErrorMessages.LoginFailed,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Succeeded = false,
                Errors = new[] { ex.Message }
            };
        }
    }

    public async Task<Response<object?>> RegisterUserAsync(RegisterRequestDto registerRequestDto)
    {
        try
        {
            // Role Validation
            if (registerRequestDto.RoleId != 1 && registerRequestDto.RoleId != 2)
            {
                return new Response<object?>
                {
                    Message = ErrorMessages.InvalidRole,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Succeeded = false
                };
            }

            // Check if user exists
            bool exists = await _userRepository.EmailOrUsernameExistsAsync(registerRequestDto.Email, registerRequestDto.Username);
            if (exists)
            {
                return new Response<object?>
                {
                    Message = ErrorMessages.EmailOrUsernameExists,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Succeeded = false,
                    Errors = new[] { ErrorMessages.EmailOrUsernameExists }
                };
            }

            // Create user
            User? user = new User
            {
                Username = registerRequestDto.Username,
                Email = registerRequestDto.Email,
                Firstname = registerRequestDto.Firstname,
                Lastname = registerRequestDto.Lastname,
                Contactnumber = registerRequestDto.Contactnumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequestDto.Password),
                RoleId = registerRequestDto.RoleId ?? 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Address = registerRequestDto.Address,
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Return successful response
            RegisterRequestDto? data = new RegisterRequestDto
            {
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Contactnumber = user.Contactnumber,
                Address = user.Address
            };

            return new Response<object?>
            {
                Message = SuccessMessages.UserRegistered,
                StatusCode = (int)HttpStatusCode.OK,
                Succeeded = true,
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new Response<object?>
            {
                Message = ErrorMessages.RegistrationFailed,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Succeeded = false,
                Errors = new[] { ex.Message }
            };
        }
    }

}
