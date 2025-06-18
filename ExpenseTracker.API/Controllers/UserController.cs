using System.Security.Claims;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<AuthController> logger)
    {
        _logger = logger;
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
                return NotFound(ErrorMessages.UserNotFound);
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user profile.");
            return StatusCode(500, "Internal server error.");
        }
    }


}
