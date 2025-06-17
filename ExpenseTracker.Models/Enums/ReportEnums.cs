using System.Text.Json.Serialization;

namespace ExpenseTracker.Models.Enums
{
    public enum ReportType
    {
        Daily,
        Monthly
    }

[JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RangeType
    {
        LastMonth,
        Last3Months,
        Custom
    }
}
