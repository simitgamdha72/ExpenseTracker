using System.Net;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Service.Interface;
using ExpenseTracker.Service.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ExpenseCategoriesController : ControllerBase
{

    private readonly IExpenseCategoriesService _expenseCategoriesService;

    public ExpenseCategoriesController(IExpenseCategoriesService expenseCategoriesService)
    {
        _expenseCategoriesService = expenseCategoriesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        Response<object>? result = await _expenseCategoriesService.GetCategoriesWithResponseAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        Response<object>? result = await _expenseCategoriesService.GetCategoryWithResponseAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [Trim]
    public async Task<IActionResult> CreateCategory(ExpenseCategoryDto expenseCategoryDto)
    {
        Response<object>? result = await _expenseCategoriesService.CreateCategoryWithResponseAsync(expenseCategoryDto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    [Trim]
    public async Task<IActionResult> UpdateCategory(int id, ExpenseCategoryDto expenseCategoryDto)
    {
        Response<object>? result = await _expenseCategoriesService.UpdateCategoryWithResponseAsync(id, expenseCategoryDto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        Response<object>? result = await _expenseCategoriesService.DeleteCategoryWithResponseAsync(id);
        return StatusCode(result.StatusCode, result);
    }

}
