namespace ExpenseTracker.Models.Dto;

public class UserExpenseSummaryDto
{
    public Dictionary<string, decimal>? CategoryWiseExpense { get; set; }
    public decimal? TotalExpense { get; set; }

}

public class FilteredExpenseSummaryDto
{
    public string? SummaryType { get; set; }
    public string? Username { get; set; }
    public Dictionary<string, UserExpenseSummaryDto> PeriodWiseSummary { get; set; } = new();
}
