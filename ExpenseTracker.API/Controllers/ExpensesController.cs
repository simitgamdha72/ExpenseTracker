using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Models.Dto;
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
        Response<object>? result = await _expensesService.GetExpensesWithResponseAsync(User);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpense(int id)
    {
        Response<object>? response = await _expensesService.GetExpenseResponseByIdAsync(id, User);
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseDto expenseDto)
    {
        Response<object>? response = await _expensesService.CreateExpenseResponseAsync(expenseDto, User);
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = "User")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseDto expenseDto)
    {
        Response<object>? response = await _expensesService.UpdateExpenseResponseAsync(id, expenseDto, User);
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = "User")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        Response<object>? response = await _expensesService.DeleteExpenseResponseAsync(id, User);
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUserExpenses([FromQuery] List<string>? userNames = null)
    {
        Response<FilteredExpenseReportDto>? response = await _expensesService.GetAllUsersExpensesResponseAsync(userNames);
        return StatusCode(response.StatusCode, response);
    }


}
