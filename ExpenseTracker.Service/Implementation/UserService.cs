using System.Net;
using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;

namespace ExpenseTracker.Service.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // public async Task<UserProfileResponseDto?> GetUserByIdAsync(int? userId)
    // {
    //     return await _userRepository.GetByIdAsync(userId);
    // }

    public async Task<Response<object>> GetUserProfileAsync(ClaimsPrincipal user)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
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
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

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
