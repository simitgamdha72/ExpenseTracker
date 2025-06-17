using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models.Validations;

namespace ExpenseTracker.Models.Dto;

public class ExpenseDto : IValidatableObject
{
    public int? Id { get; set; }

    [Required(ErrorMessage = RequiredValidationMessages.CategoryRequired)]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = ValidationMessages.CategoryLettersOnly)]
    public string? Category { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = ValidationMessages.PositiveAmount)]
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }

    [Required(ErrorMessage = RequiredValidationMessages.NoteRequired)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = ValidationMessages.NoteCannotBeSpaces)]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpenseDate > DateOnly.FromDateTime(DateTime.Now))
        {
            yield return new ValidationResult(ValidationMessages.FutureDateNotAllowed, new[] { nameof(ExpenseDate) });
        }
    }
}
