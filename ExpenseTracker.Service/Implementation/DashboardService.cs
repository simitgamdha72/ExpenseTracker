using System.Net;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Models.Dto;
using ExpenseTracker.Models.Enums;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Validations.Constants.ErrorMessages;
using ExpenseTracker.Models.Validations.Constants.SuccessMessages;
using ExpenseTracker.Repository.Interface;
using ExpenseTracker.Service.Interface;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Service.Implementation;

public class DashboardService : IDashboardService
{

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IExpenseRepository expenseRepository, IExpenseReportRepository expenseReportRepository, ILogger<DashboardService> logger)
    {
        _expenseRepository = expenseRepository;
        _expenseReportRepository = expenseReportRepository;
        _logger = logger;
    }

    public Response<object?> GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        try
        {
            _logger.LogInformation("Fetching expense summary. ReportType: {ReportType}, RangeType: {RangeType}",
              csvExportFilterRequestDto.ReportType, csvExportFilterRequestDto.RangeType);

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (csvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.StartDate > today)
                {
                    _logger.LogWarning("Start date is in the future: {StartDate}", csvExportFilterRequestDto.StartDate);
                    return BadRequestResponse(ErrorMessages.StartDateInFuture);
                }

                if (csvExportFilterRequestDto.EndDate.HasValue && csvExportFilterRequestDto.EndDate > today)
                {
                    _logger.LogWarning("End date is in the future: {EndDate}", csvExportFilterRequestDto.EndDate);
                    return BadRequestResponse(ErrorMessages.EndDateInFuture);
                }

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue &&
                        csvExportFilterRequestDto.StartDate > csvExportFilterRequestDto.EndDate)
                {
                    _logger.LogWarning("Start date is after end date: {StartDate} > {EndDate}",
                       csvExportFilterRequestDto.StartDate, csvExportFilterRequestDto.EndDate);
                    return BadRequestResponse(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (csvExportFilterRequestDto.ReportType == ReportType.Monthly &&
                csvExportFilterRequestDto.RangeType == RangeType.Custom)
            {
                bool startValid = csvExportFilterRequestDto.StartMonth.HasValue && csvExportFilterRequestDto.StartYear.HasValue;
                bool endValid = csvExportFilterRequestDto.EndMonth.HasValue && csvExportFilterRequestDto.EndYear.HasValue;

                if (!startValid || !endValid)
                {
                    _logger.LogWarning("Custom month range required but not provided.");
                    return BadRequestResponse(ErrorMessages.CustomMonthRangeRequired);
                }

                DateOnly start = new DateOnly(csvExportFilterRequestDto.StartYear!.Value, csvExportFilterRequestDto.StartMonth!.Value, 1);
                DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear!.Value, csvExportFilterRequestDto.EndMonth!.Value,
                    DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));

                if (start > today)
                {
                    _logger.LogWarning("Start month is in the future: {Start}", start);
                    return BadRequestResponse(ErrorMessages.StartMonthInFuture);
                }

                if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                {
                    _logger.LogWarning("End month is in the future: {End}", end);
                    return BadRequestResponse(ErrorMessages.EndMonthInFuture);
                }

                if (start > end)
                {
                    _logger.LogWarning("Start month is after end month: {Start} > {End}", start, end);
                    return BadRequestResponse(ErrorMessages.StartMonthAfterEndMonth);
                }
            }

            _logger.LogInformation("Retrieving filtered expenses.");

            // Proceed to summary generation
            IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredExpenses(csvExportFilterRequestDto);
            bool isMonthly = csvExportFilterRequestDto.ReportType == ReportType.Monthly;

            var groupedByCategory = expenses
                .GroupBy(exp => exp.Category?.Name ?? "Uncategorized")
                .Select(group => new
                {
                    Category = group.Key,
                    TotalAmount = group.Sum(e => e.Amount),
                    Expenses = group.Select(e => new
                    {
                        Username = e.User?.Username,
                        Date = isMonthly
                            ? e.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                            : e.ExpenseDate.ToString("yyyy-MM-dd"),
                        Amount = e.Amount,
                        Note = e.Note
                    }).ToList()
                }).ToList();

            decimal grandTotal = groupedByCategory.Sum(g => g.TotalAmount);

            var summary = new
            {
                Categories = groupedByCategory,
                TotalExpense = grandTotal
            };

            _logger.LogInformation("Expense summary generated successfully. Category count: {CategoryCount}, Grand total: {GrandTotal}",
              groupedByCategory.Count, grandTotal);

            return new Response<object?>
            {
                Message = SuccessMessages.SummaryDataFetched,
                StatusCode = (int)HttpStatusCode.OK,
                Succeeded = true,
                Data = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the expense summary.");
            return new Response<object?>
            {
                Message = ErrorMessages.GetSummaryFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Errors = new[] { ex.Message }
            };
        }
    }

    private Response<object?> BadRequestResponse(string errorMessage)
    {
        _logger.LogWarning("BadRequest returned: {ErrorMessage}", errorMessage);

        return new Response<object?>
        {
            Succeeded = false,
            StatusCode = (int)HttpStatusCode.BadRequest,
            Errors = new[] { errorMessage }
        };
    }

    public Response<object?> ExportExpensesToCsv(CsvExportFilterRequestDto dto, ClaimsPrincipal user)
    {
        try
        {
            _logger.LogInformation("Starting CSV export for user.");

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("CSV export failed: User is not authenticated.");

                return new Response<object?>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
            if (userId == 0)
            {
                _logger.LogWarning("CSV export failed: Invalid or missing user ID.");
                return new Response<object?>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            _logger.LogInformation("Validating report filters for userId: {UserId}", userId);

            // Validation
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (dto.ReportType == ReportType.Daily)
            {
                if (dto.StartDate > today)
                {
                    _logger.LogWarning("Start date is in the future: {StartDate}", dto.StartDate);
                    return Error(ErrorMessages.StartDateInFuture);
                }
                if (dto.EndDate > today)
                {
                    _logger.LogWarning("End date is in the future: {EndDate}", dto.EndDate);
                    return Error(ErrorMessages.EndDateInFuture);
                }
                if (dto.StartDate > dto.EndDate)
                {
                    _logger.LogWarning("Start date is after end date: {StartDate} > {EndDate}", dto.StartDate, dto.EndDate);
                    return Error(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (dto.ReportType == ReportType.Monthly && dto.RangeType == RangeType.Custom)
            {
                if (!dto.StartMonth.HasValue || !dto.StartYear.HasValue || !dto.EndMonth.HasValue || !dto.EndYear.HasValue)
                {
                    _logger.LogWarning("Custom month range is incomplete.");
                    return Error(ErrorMessages.CustomMonthRangeRequired);
                }

                DateOnly start = new DateOnly(dto.StartYear.Value, dto.StartMonth.Value, 1);
                DateOnly end = new DateOnly(dto.EndYear.Value, dto.EndMonth.Value,
                    DateTime.DaysInMonth(dto.EndYear.Value, dto.EndMonth.Value));

                if (start > today)
                {
                    _logger.LogWarning("Start month is in the future: {Start}", start);
                    return Error(ErrorMessages.StartMonthInFuture);
                }
                if (end > today)
                {
                    _logger.LogWarning("End month is in the future: {End}", end);
                    return Error(ErrorMessages.EndMonthInFuture);
                }
                if (start > end)
                {
                    _logger.LogWarning("Start month is after end month: {Start} > {End}", start, end);
                    return Error(ErrorMessages.StartMonthAfterEndMonth);
                }
            }

            _logger.LogInformation("Fetching expenses for CSV generation.");

            // Generate CSV
            IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredExpenses(dto);

            StringBuilder? csv = new StringBuilder();

            bool isMonthly = dto.ReportType == ReportType.Monthly;

            csv.AppendLine(isMonthly
                ? "\"Username\",\"Month\",\"Category\",\"Amount\",\"Note\""
                : "\"Username\",\"Date\",\"Category\",\"Amount\",\"Note\"");

            Dictionary<string, decimal>? categoryTotals = new Dictionary<string, decimal>();
            decimal grandTotal = 0;

            foreach (var exp in expenses)
            {
                string username = Sanitize(exp.User?.Username ?? "Unknown");
                string category = Sanitize(exp.Category?.Name ?? "Uncategorized");
                string note = Sanitize(exp.Note ?? "");
                string date = isMonthly
                    ? exp.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                    : exp.ExpenseDate.ToString("yyyy-MM-dd");

                csv.AppendLine($"\"{username}\",\"{date}\",\"{category}\",\"{exp.Amount}\",\"{note}\"");

                categoryTotals[category] = categoryTotals.GetValueOrDefault(category, 0) + exp.Amount;
                grandTotal += exp.Amount;
            }

            csv.AppendLine();
            csv.AppendLine("\"--- Category Totals ---\"");
            csv.AppendLine("\"Category\",\"Total Amount\"");
            foreach (var entry in categoryTotals)
                csv.AppendLine($"\"{entry.Key}\",\"{entry.Value}\"");

            csv.AppendLine();
            csv.AppendLine($"\"Total Expense:\",\"{grandTotal}\"");

            MemoryStream? stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));

            _logger.LogInformation("Saving export report to database for userId: {UserId}", userId);

            _expenseReportRepository.AddAsync(new ExpenseReport { UserId = userId });
            _expenseReportRepository.SaveChangesAsync();

            _logger.LogInformation("CSV export successful for userId: {UserId}. Total: {TotalAmount}", userId, grandTotal);

            return new Response<object?>
            {
                Message = SuccessMessages.CsvExportSuccessful,
                StatusCode = (int)HttpStatusCode.OK,
                Succeeded = true,
                Data = stream
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while exporting expenses to CSV.");
            
            return new Response<object?>
            {
                Message = ErrorMessages.ExportCsvFailed,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Succeeded = false,
                Errors = new[] { ex.Message }
            };
        }

        Response<object?> Error(string message) => new()
        {
            Succeeded = false,
            StatusCode = (int)HttpStatusCode.BadRequest,
            Errors = new[] { message }
        };

        string Sanitize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value.Replace("\"", "\"\"");
    }



}
