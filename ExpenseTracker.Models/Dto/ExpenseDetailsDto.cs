namespace ExpenseTracker.Models.Dto;

public class ExpenseDetailsDto
{
    public string? UserName { get; set; }

    public ExpenseDto expenseDto { get; set; } = new ExpenseDto();

}

