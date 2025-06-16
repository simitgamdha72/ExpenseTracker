using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Models.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
                return NotFound("User not found!");
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            IEnumerable<ExpenseDto> Expenses = await _expensesService.GetExpensesByUserIdAsync(userId);
            return Ok(Expenses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving expenses: {ex.Message}");
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
                return NotFound("User not found!");
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            ExpenseDto? expense = await _expensesService.GetExpenseByIdAsync(id, userId);

            if (expense == null)
            {
                return NotFound("Expense not found");
            }

            return Ok(expense);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving the expense: {ex.Message}");
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
                return NotFound("User not found!");
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            (bool Success, string Message) result = await _expensesService.CreateExpenseAsync(userId, expenseDto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return StatusCode(201, result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while creating the expense: {ex.Message}");
        }
    }

    [Authorize(Roles = "User")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseDto expenseDto)
    {
        if (id != expenseDto.Id)
            return BadRequest("Expense ID mismatch");
        try
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return NotFound("User not found!");
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            (bool Success, string Message) result = await _expensesService.UpdateExpenseAsync(id, userId, expenseDto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating the expense: {ex.Message}");
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
                return NotFound("User not found!");
            }

            int? userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (userId == 0)
            {
                return NotFound();
            }

            (bool Success, string Message) result = await _expensesService.DeleteExpenseAsync(id, userId);

            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while deleting the expense: {ex.Message}");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUserExpenses([FromQuery] List<string>? userNames = null)
    {
        try
        {
            FilteredExpenseReportDto expenses = await _expensesService.GetAllUsersExpensesAsync(userNames);

            return Ok(expenses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving expenses: {ex.Message}");
        }
    }

}
