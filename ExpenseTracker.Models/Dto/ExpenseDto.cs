using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dto;

public class ExpenseDto : IValidatableObject
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Category is Required")]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = "Category must contain only letters and cannot be just spaces.")]
    public string? Category { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive number greater than 0.")]
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }

    [Required(ErrorMessage = "Note is Required")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Note cannot be only spaces.")]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpenseDate > DateOnly.FromDateTime(DateTime.Now))
        {
            yield return new ValidationResult("Future dates are not allowed for expenses.", new[] { nameof(ExpenseDate) });
        }
    }
}
