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
    public async Task<ActionResult<IEnumerable<ExpenseCategoryDto>>> GetCategories()
    {
        try
        {
            Response<object> response = new Response<object>
            {
                Message = SuccessMessages.CategoriesFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = await _expenseCategoriesService.GetCategoriesAsync()
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseCategoryDto>> GetCategory(int id)
    {
        try
        {
            ExpenseCategoryDto? expenseCategoryDto = await _expenseCategoriesService.GetCategoryAsync(id);

            if (expenseCategoryDto == null)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.NotFound }
                };
                return NotFound(responseError);
            }

            Response<ExpenseCategoryDto> response = new Response<ExpenseCategoryDto>
            {
                Message = SuccessMessages.CategoryFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = expenseCategoryDto
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

    [HttpPost]
    [Trim]
    public async Task<IActionResult> CreateCategory(ExpenseCategoryDto expenseCategoryDto)
    {
        try
        {

            (bool Success, string Message, ExpenseCategory? Category) result = await _expenseCategoriesService.CreateCategoryAsync(expenseCategoryDto);

            if (!result.Success)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Data = null,
                    Errors = new[] { result.Message }
                };
                return Conflict(responseError);
            }

            Response<ExpenseCategory> response = new Response<ExpenseCategory>
            {
                Message = SuccessMessages.Created,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.Created,
                Data = result.Category
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

    [HttpPut("{id}")]
    [Trim]
    public async Task<IActionResult> UpdateCategory(int id, ExpenseCategoryDto expenseCategoryDto)
    {
        try
        {

            (bool Success, string Message, Models.Models.ExpenseCategory? Category) result = await _expenseCategoriesService.UpdateCategoryAsync(id, expenseCategoryDto);

            if (!result.Success)
            {
                Response<object> responseError = new Response<object>
                {
                    Message = ErrorMessages.CategoryNotFound,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.NotFound }
                };
                return NotFound(responseError);
            }

            Response<ExpenseCategory> response = new Response<ExpenseCategory>
            {
                Message = SuccessMessages.Updated,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = result.Category
            };

            return Ok(response);

        }
        catch (Exception ex)
        {
            if (ex.Message == ErrorMessages.CategoryNameExists)
            {
                Response<object> response = new Response<object>
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Data = null,
                    Errors = new[] { ex.Message }
                };
                return Conflict(response);
            }
            else
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            (bool Success, string Message) result = await _expenseCategoriesService.DeleteCategoryAsync(id);

            if (!result.Success)
            {
                Response<object> responseError = new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Data = null,
                    Errors = new[] { ErrorMessages.NotFound }
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
