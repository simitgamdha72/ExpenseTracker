namespace ExpenseTracker.Models.Dto;

public class UserCsvExportFilterDto
{
    public string ReportType { get; set; } = "monthly"; // "daily" or "monthly"
    public string RangeType { get; set; } = "custom";   // "lastMonth", "last3Months", "custom"

    public DateOnly? StartDate { get; set; }            // For daily
    public DateOnly? EndDate { get; set; }

    public int? StartMonth { get; set; }                // For monthly
    public int? StartYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }
}
