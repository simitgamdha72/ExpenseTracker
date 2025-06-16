using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models.Validations;

namespace ExpenseTracker.Models.Dto;

public class LoginDto
{

    [Required(ErrorMessage = RequiredValidationMessages.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationMessages.InvalidEmail)]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
ErrorMessage = ValidationMessages.EmailSpecialChar)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = RequiredValidationMessages.PasswordRequired)]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^\S+$", ErrorMessage = ValidationMessages.PasswordNoSpaces)]
    public string Password { get; set; } = "";
}