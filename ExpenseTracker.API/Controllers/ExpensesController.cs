using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Models.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using System.Net;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpensesController : ControllerBase
{
    private readonly IExpensesService _expensesService;

    public ExpensesController(IExpensesService expensesService)
    {
        _expensesService = expensesService;
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> GetUserExpenses()
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

            IEnumerable<ExpenseDto> Expenses = await _expensesService.GetExpensesByUserIdAsync(userId);

            Response<IEnumerable<ExpenseDto>> response = new Response<IEnumerable<ExpenseDto>>
            {
                Message = SuccessMessages.ExpensesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = Expenses
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.GetExpensesFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [Authorize(Roles = "User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpense(int id)
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

            ExpenseDto? expense = await _expensesService.GetExpenseByIdAsync(id, userId);

            if (expense == null)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.ExpenseNotFound,
                    Succeeded = true,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                };
                return NotFound(responseError);
            }

            Response<ExpenseDto> response = new Response<ExpenseDto>
            {
                Message = SuccessMessages.ExpensesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = expense
            };
            return Ok(response);

        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.GetExpensesFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseDto expenseDto)
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

            (bool Success, string Message, Expense? expense) result = await _expensesService.CreateExpenseAsync(userId, expenseDto);

            if (!result.Success)
            {

                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.InvalidCredentials,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                    Errors = new[] { result.Message }
                };
                return BadRequest(responseError);
            }

            Response<Expense> response = new Response<Expense>
            {
                Message = SuccessMessages.ExpenseCreated,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.Created,
                Data = result.expense
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.CreateExpenseFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [Authorize(Roles = "User")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseDto expenseDto)
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

            (bool Success, string Message, Expense? expense) result = await _expensesService.UpdateExpenseAsync(id, userId, expenseDto);

            if (!result.Success)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.InvalidCredentials,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null,
                    Errors = new[] { result.Message }
                };
                return BadRequest(responseError);
            }

            Response<Expense> response = new Response<Expense>
            {
                Message = SuccessMessages.ExpenseUpdated,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = result.expense
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.UpdateExpenseFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [Authorize(Roles = "User")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
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

            (bool Success, string Message) result = await _expensesService.DeleteExpenseAsync(id, userId);

            if (!result.Success)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.InvalidCredentials,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { result.Message }
                };
                return NotFound(responseError);
            }
            Response<object> response = new Response<object>
            {
                Message = result.Message,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.DeleteExpenseFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUserExpenses([FromQuery] List<string>? userNames = null)
    {
        try
        {
            FilteredExpenseReportDto expenses = await _expensesService.GetAllUsersExpensesAsync(userNames);

            Response<FilteredExpenseReportDto> response = new Response<FilteredExpenseReportDto>
            {
                Message = SuccessMessages.ExpensesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = expenses
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.GetExpensesFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

}
