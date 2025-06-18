using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
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
    public async Task<ActionResult<IEnumerable<ExpenseCategoryDto>>> GetCategories()
    {
        try
        {
            return Ok(await _expenseCategoriesService.GetCategoriesAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.InternalServerError, detail = ex.Message });
        }

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseCategoryDto>> GetCategory(int id)
    {
        try
        {
            ExpenseCategoryDto? expenseCategoryDto = await _expenseCategoriesService.GetCategoryAsync(id);

            if (expenseCategoryDto == null)
            {
                return NotFound(ErrorMessages.CategoryNotFound);
            }

            return Ok(expenseCategoryDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.InternalServerError, detail = ex.Message });
        }

    }

    [HttpPost]
    [Trim]
    public async Task<IActionResult> CreateCategory(ExpenseCategoryDto expenseCategoryDto)
    {
        try
        {

            (bool Success, string Message, Models.Models.ExpenseCategory? Category) result = await _expenseCategoriesService.CreateCategoryAsync(expenseCategoryDto);

            if (!result.Success)
            {
                return Conflict(result.Message);
            }

            return CreatedAtAction(nameof(GetCategory), new { id = result.Category!.Id }, result.Category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.InternalServerError, detail = ex.Message });
        }

    }

    [HttpPut("{id}")]
    [Trim]
    public async Task<IActionResult> UpdateCategory(int id, ExpenseCategoryDto expenseCategoryDto)
    {
        try
        {

            (bool Success, string Message, Models.Models.ExpenseCategory? Category) result = await _expenseCategoriesService.UpdateCategoryAsync(id, expenseCategoryDto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return CreatedAtAction(nameof(GetCategory), new { id = result.Category!.Id }, result.Category);

        }
        catch (Exception ex)
        {
            if (ex.Message == ErrorMessages.CategoryNameExists)
            {
                return Conflict(ErrorMessages.CategoryNameExists);
            }
            else
            {
                return StatusCode(500, new { message = ErrorMessages.InternalServerError, detail = ex.Message });
            }


        }

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            (bool Success, string Message) result = await _expenseCategoriesService.DeleteCategoryAsync(id);

            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ErrorMessages.InternalServerError, detail = ex.Message });
        }

    }
}
