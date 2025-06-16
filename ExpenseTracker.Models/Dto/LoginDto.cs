using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dto;

public class LoginDto
{

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
ErrorMessage = "Email cannot start with a special character")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "password is required")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^\S+$", ErrorMessage = "Password cannot contain spaces.")]
    public string Password { get; set; } = "";
}