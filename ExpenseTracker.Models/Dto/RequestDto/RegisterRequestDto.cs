using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models.Validations;

namespace ExpenseTracker.Models.Dto;

public class RegisterRequestDto
{
    [Required(ErrorMessage = RequiredValidationMessages.UsernameRequired)]
    [RegularExpression(@"^\S+$", ErrorMessage = ValidationMessages.NoSpacesUsername)]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = RequiredValidationMessages.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationMessages.InvalidEmail)]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
   ErrorMessage = ValidationMessages.EmailSpecialChar)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = RequiredValidationMessages.PasswordRequired)]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = ValidationMessages.PasswordMinLength)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])\S{8,}$",
    ErrorMessage = ValidationMessages.PasswordRequirements)]
    public string Password { get; set; } = "";

    [Required]
    public int? RoleId { get; set; }

    [Required(ErrorMessage = RequiredValidationMessages.FirstNameRequired)]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = ValidationMessages.FirstNameOnlyLetters)]
    public string Firstname { get; set; } = null!;

    [Required(ErrorMessage = RequiredValidationMessages.LastNameRequired)]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = ValidationMessages.LastNameOnlyLetters)]
    public string Lastname { get; set; } = null!;

    [Required(ErrorMessage = RequiredValidationMessages.PhoneRequired)]
    [DataType(DataType.PhoneNumber)]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = ValidationMessages.ContactnumberFormat)]
    public string Contactnumber { get; set; } = null!;

    public string? Address { get; set; }
}