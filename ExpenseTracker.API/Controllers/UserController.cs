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
        var response = await _userService.GetUserProfileAsync(User);
        return StatusCode(response.StatusCode, response);
    }



}
