using System.Security.Claims;
using ExpenseTracker.Models.Dto;

namespace ExpenseTracker.Service.Interface;

public interface IUserService
{
    Task<Response<object>> GetUserProfileAsync(ClaimsPrincipal user);

}
