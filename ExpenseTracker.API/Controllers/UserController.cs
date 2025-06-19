using System.Net;
using System.Security.Claims;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpGet("my-profile")]
    [Authorize]
    public async Task<IActionResult> MyProfile()
    {
        try
        {
            if (!User.Identity!.IsAuthenticated)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
                return NotFound(responseError);
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
                return NotFound(responseError);
            }

            UserProfileResponseDto? user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
                return NotFound(responseError);
            }

            Response<object> response = new Response<object>
            {
                Message = SuccessMessages.SummaryDataFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = user
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.InternalServerError,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };

            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }


}
