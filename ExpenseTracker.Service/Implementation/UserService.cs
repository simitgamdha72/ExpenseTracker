using System.Net;
using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Service.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Response<object>> GetUserProfileAsync(ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to GetUserProfile.");

                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                _logger.LogWarning("User ID parsing failed or is 0 in GetUserProfile.");

                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            UserProfileResponseDto? profile = await _userRepository.GetByIdAsync(userId);

            if (profile == null)
            {
                _logger.LogWarning("User profile not found. UserId: {UserId}", userId);

                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            _logger.LogInformation("User profile fetched successfully. UserId: {UserId}", userId);

            return new Response<object>
            {
                Message = SuccessMessages.ProfileFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = profile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred in GetUserProfileAsync.");

            return new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Errors = new[] { ex.Message }
            };
        }
    }

}
