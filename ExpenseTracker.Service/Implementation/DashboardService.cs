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

namespace ExpenseTracker.Service.Implementation;

public class DashboardService : IDashboardService
{

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseReportRepository _expenseReportRepository;

    public DashboardService(IExpenseRepository expenseRepository, IExpenseReportRepository expenseReportRepository)
    {
        _expenseRepository = expenseRepository;
        _expenseReportRepository = expenseReportRepository;
    }

    public Response<object?> GetExpenseSummary(CsvExportFilterRequestDto csvExportFilterRequestDto)
    {
        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (csvExportFilterRequestDto.ReportType == ReportType.Daily)
            {
                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.StartDate > today)
                {
                    return BadRequestResponse(ErrorMessages.StartDateInFuture);
                }

                if (csvExportFilterRequestDto.EndDate.HasValue && csvExportFilterRequestDto.EndDate > today)
                {
                    return BadRequestResponse(ErrorMessages.EndDateInFuture);
                }

                if (csvExportFilterRequestDto.StartDate.HasValue && csvExportFilterRequestDto.EndDate.HasValue &&
                        csvExportFilterRequestDto.StartDate > csvExportFilterRequestDto.EndDate)
                {
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
                    return BadRequestResponse(ErrorMessages.CustomMonthRangeRequired);
                }

                DateOnly start = new DateOnly(csvExportFilterRequestDto.StartYear!.Value, csvExportFilterRequestDto.StartMonth!.Value, 1);
                DateOnly end = new DateOnly(csvExportFilterRequestDto.EndYear!.Value, csvExportFilterRequestDto.EndMonth!.Value,
                    DateTime.DaysInMonth(csvExportFilterRequestDto.EndYear.Value, csvExportFilterRequestDto.EndMonth.Value));

                if (start > today)
                {
                    return BadRequestResponse(ErrorMessages.StartMonthInFuture);
                }

                if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                {
                    return BadRequestResponse(ErrorMessages.EndMonthInFuture);
                }

                if (start > end)
                {
                    return BadRequestResponse(ErrorMessages.StartMonthAfterEndMonth);
                }
            }

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
            if (!user.Identity?.IsAuthenticated ?? true)
            {
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
                return new Response<object?>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            // Validation
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (dto.ReportType == ReportType.Daily)
            {
                if (dto.StartDate > today)
                {
                    return Error(ErrorMessages.StartDateInFuture);
                }
                if (dto.EndDate > today)
                {
                    return Error(ErrorMessages.EndDateInFuture);
                }
                if (dto.StartDate > dto.EndDate)
                {
                    return Error(ErrorMessages.StartDateAfterEndDate);
                }
            }

            if (dto.ReportType == ReportType.Monthly && dto.RangeType == RangeType.Custom)
            {
                if (!dto.StartMonth.HasValue || !dto.StartYear.HasValue || !dto.EndMonth.HasValue || !dto.EndYear.HasValue)
                {
                    return Error(ErrorMessages.CustomMonthRangeRequired);
                }

                DateOnly start = new DateOnly(dto.StartYear.Value, dto.StartMonth.Value, 1);
                DateOnly end = new DateOnly(dto.EndYear.Value, dto.EndMonth.Value,
                    DateTime.DaysInMonth(dto.EndYear.Value, dto.EndMonth.Value));

                if (start > today)
                {
                    return Error(ErrorMessages.StartMonthInFuture);
                }
                if (end > today)
                {
                    return Error(ErrorMessages.EndMonthInFuture);
                }
                if (start > end)
                {
                    return Error(ErrorMessages.StartMonthAfterEndMonth);
                }
            }

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

            _expenseReportRepository.AddAsync(new ExpenseReport { UserId = userId });
            _expenseReportRepository.SaveChangesAsync();

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
