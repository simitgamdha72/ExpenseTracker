namespace ExpenseTracker.Models.Dto;

public class UserProfileResponseDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Contactnumber { get; set; } = null!;
    public string? Address { get; set; }
    public string RoleName { get; set; } = null!;
}
