using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dto;

public class RegisterDto
{
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
   ErrorMessage = "Email cannot start with a special character")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])\S{8,}$",
    ErrorMessage = "Password must be at least 8 characters, include uppercase, lowercase, number, special character, and have no spaces.")]
    public string Password { get; set; } = "";

    [Required]
    public int? RoleId { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = "FirstName must contain only letters and cannot be just spaces.")]
    public string Firstname { get; set; } = null!;

    [Required(ErrorMessage = "LastName is required")]
    [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z\s]+$", ErrorMessage = "LastName must contain only letters and cannot be just spaces.")]
    public string Lastname { get; set; } = null!;

    [Required(ErrorMessage = "Phone is required")]
    [DataType(DataType.PhoneNumber)]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Phone number must be exactly 10 digits and cannot start with 0.")]
    public string Phone { get; set; } = null!;

    public string? Address { get; set; }
}