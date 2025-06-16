using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models.Validations;

namespace ExpenseTracker.Models.Dto;

public class ExpenseCategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = RequiredValidationMessages.CategoryRequired)]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = ValidationMessages.NameOnlyLetters)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = RequiredValidationMessages.DescriptionRequired)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = ValidationMessages.DescriptionCannotBeSpaces)]
    public string? Description { get; set; }

}
