namespace ExpenseTracker.Models.Dto;

public class CsvExportFilterDto
{
    public string? Username { get; set; }
    public string? Category { get; set; }
    public string ReportType { get; set; } = "monthly";

    public string? RangeType { get; set; } // "lastMonth", "last3Months", "custom"
    public int? StartMonth { get; set; }
    public int? StartYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
