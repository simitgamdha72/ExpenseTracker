using ExpenseTracker.Models.Enums;

namespace ExpenseTracker.Models.Dto;

public class UserCsvExportFilterRequestDto
{
    public ReportType ReportType { get; set; }
    public RangeType RangeType { get; set; }  // "lastMonth", "last3Months", "custom"

    public DateOnly? StartDate { get; set; }            // For daily
    public DateOnly? EndDate { get; set; }

    public int? StartMonth { get; set; }                // For monthly
    public int? StartYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }
}
