namespace ExpenseTracker.Models.Dto;

public class FilteredExpenseReportDto
{
    public List<ExpenseDetailsDto> Expenses { get; set; } = new();
    public List<string> NotFoundUsernames { get; set; } = new();
}