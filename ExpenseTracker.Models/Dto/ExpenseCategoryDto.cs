using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dto;

public class ExpenseCategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category Name is Required")]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = "Name must contain only letters and cannot be just spaces.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is Required")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Description cannot be only spaces.")]
    public string? Description { get; set; }

}
