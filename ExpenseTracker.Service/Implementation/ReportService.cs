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

public class ReportService : IReportService
{

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseReportRepository _expenseReportRepository;

    public ReportService(IExpenseRepository expenseRepository, IExpenseReportRepository expenseReportRepository)
    {
        _expenseRepository = expenseRepository;
        _expenseReportRepository = expenseReportRepository;
    }

    public (MemoryStream FileStream, Response<object> Response) ExportUserExpensesToCsv(ClaimsPrincipal user, UserCsvExportFilterRequestDto filterDto)
    {
        if (!user.Identity!.IsAuthenticated)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.UnauthorizedAccess,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Errors = new[] { ErrorMessages.UserNotFound }
            };
            return (new MemoryStream(), response);
        }

        int userId = int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        if (userId == 0)
        {
            Response<object> response = new Response<object>
            {
                Message = ErrorMessages.UnauthorizedAccess,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Errors = new[] { ErrorMessages.UserNotFound }
            };
            return (new MemoryStream(), response);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Validate daily report
        if (filterDto.ReportType == ReportType.Daily)
        {
            if (filterDto.StartDate > today || filterDto.EndDate > today)
            {
                return (new MemoryStream(), new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.FutureDateNotAllowed }
                });
            }

            if (filterDto.StartDate > filterDto.EndDate)
            {
                return (new MemoryStream(), new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.StartDateAfterEndDate }
                });
            }
        }

        // Validate monthly custom range
        if (filterDto.ReportType == ReportType.Monthly && filterDto.RangeType == RangeType.Custom)
        {
            if (filterDto.StartMonth.HasValue && filterDto.StartYear.HasValue &&
                filterDto.EndMonth.HasValue && filterDto.EndYear.HasValue)
            {
                DateOnly start = new DateOnly(filterDto.StartYear.Value, filterDto.StartMonth.Value, 1);
                DateOnly end = new DateOnly(filterDto.EndYear.Value, filterDto.EndMonth.Value,
                    DateTime.DaysInMonth(filterDto.EndYear.Value, filterDto.EndMonth.Value));

                if (start > today)
                {
                    return (new MemoryStream(), new Response<object>
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Errors = new[] { ErrorMessages.FutureMonthNotAllowed }
                    });
                }

                if (end > today)
                {
                    return (new MemoryStream(), new Response<object>
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Errors = new[] { ErrorMessages.EndMonthInFuture }
                    });
                }

                if (start > end)
                {
                    return (new MemoryStream(), new Response<object>
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Errors = new[] { ErrorMessages.StartMonthAfterEndMonth }
                    });
                }
            }
            else
            {
                return (new MemoryStream(), new Response<object>
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new[] { ErrorMessages.CustomMonthRangeRequired }
                });
            }
        }

        // Generate CSV
        IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredUserExpenses(userId, filterDto);

        StringBuilder csv = new();
        csv.AppendLine("\"Date\",\"Category\",\"Amount\",\"Note\"");

        Dictionary<string, decimal> categoryTotals = new();
        decimal total = 0;

        foreach (var e in expenses)
        {
            string category = SanitizeForCsv(e.Category?.Name ?? "Uncategorized");
            string note = SanitizeForCsv(e.Note ?? "");
            string date = e.ExpenseDate.ToString("yyyy-MM-dd");
            string amount = e.Amount.ToString();

            csv.AppendLine($"\"{date}\",\"{category}\",\"{amount}\",\"{note}\"");

            if (categoryTotals.ContainsKey(category))
                categoryTotals[category] += e.Amount;
            else
                categoryTotals[category] = e.Amount;

            total += e.Amount;
        }

        // Category Totals
        csv.AppendLine();
        csv.AppendLine("\"--- Category Totals ---\"");
        csv.AppendLine("\"Category\",\"TotalAmount\"");
        foreach (var c in categoryTotals)
            csv.AppendLine($"\"{c.Key}\",\"{c.Value}\"");

        // Grand Total
        csv.AppendLine();
        csv.AppendLine($"\"Total Expense:\",\"{total}\"");

        // Log report generation
        ExpenseReport reportLog = new()
        {
            UserId = userId
        };
        _expenseReportRepository.AddAsync(reportLog);
        _expenseReportRepository.SaveChangesAsync();

        MemoryStream? fileStream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));

        return (fileStream, new Response<object>
        {
            Succeeded = true,
            StatusCode = (int)HttpStatusCode.OK
        });
    }

    private static string SanitizeForCsv(string input)
    {
        return string.IsNullOrEmpty(input) ? "" : input.Replace("\"", "\"\"");
    }

    public Response<object> GetUserExpenseSummary(ClaimsPrincipal user, UserCsvExportFilterRequestDto filterDto)
    {
        try
        {
            if (!user.Identity!.IsAuthenticated)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            int userId = int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

            if (userId == 0)
            {
                return new Response<object>
                {
                    Message = ErrorMessages.UnauthorizedAccess,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Errors = new[] { ErrorMessages.UserNotFound }
                };
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (filterDto.ReportType == ReportType.Daily)
            {
                if (filterDto.StartDate > today || filterDto.EndDate > today)
                {
                    return new Response<object>
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Errors = new[] { ErrorMessages.FutureDateNotAllowed }
                    };
                }

                if (filterDto.StartDate > filterDto.EndDate)
                {
                    return new Response<object>
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Errors = new[] { ErrorMessages.StartDateAfterEndDate }
                    };
                }
            }

            if (filterDto.ReportType == ReportType.Monthly && filterDto.RangeType == RangeType.Custom)
            {
                if (filterDto.StartMonth.HasValue && filterDto.StartYear.HasValue &&
                    filterDto.EndMonth.HasValue && filterDto.EndYear.HasValue)
                {
                    DateOnly start = new DateOnly(filterDto.StartYear.Value, filterDto.StartMonth.Value, 1);
                    DateOnly end = new DateOnly(filterDto.EndYear.Value, filterDto.EndMonth.Value,
                        DateTime.DaysInMonth(filterDto.EndYear.Value, filterDto.EndMonth.Value));

                    if (start > today)
                    {
                        return new Response<object>
                        {
                            Succeeded = false,
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Errors = new[] { ErrorMessages.FutureMonthNotAllowed }
                        };
                    }
                    if (end.Year > today.Year || (end.Year == today.Year && end.Month > today.Month))
                    {
                        return new Response<object>
                        {
                            Succeeded = false,
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Errors = new[] { ErrorMessages.EndMonthInFuture }
                        };
                    }
                    if (start > end)
                    {
                        return new Response<object>
                        {
                            Succeeded = false,
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Errors = new[] { ErrorMessages.StartMonthAfterEndMonth }
                        };
                    }
                }
                else
                {
                    return new Response<object>
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Errors = new[] { ErrorMessages.CustomMonthRangeRequired }
                    };
                }
            }

            IEnumerable<Expense>? expenses = _expenseRepository.GetFilteredUserExpenses(userId, filterDto);

            bool isMonthly = filterDto.ReportType == ReportType.Monthly;

            var groupedByCategory = expenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .Select(group => new
                {
                    Category = group.Key,
                    TotalAmount = group.Sum(e => e.Amount),
                    Expenses = group.Select(e => new
                    {
                        Date = isMonthly
                            ? e.ExpenseDate.ToDateTime(new TimeOnly(0)).ToString("MMMM yyyy")
                            : e.ExpenseDate.ToString("yyyy-MM-dd"),
                        Amount = e.Amount,
                        Note = e.Note
                    }).ToList()
                }).ToList();

            decimal totalExpense = groupedByCategory.Sum(g => g.TotalAmount);

            return new Response<object>
            {
                Message = SuccessMessages.SummaryDataFetched,
                Succeeded = true,
                StatusCode = (int)HttpStatusCode.OK,
                Data = new
                {
                    Categories = groupedByCategory,
                    TotalExpense = totalExpense
                }
            };

        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Message = ErrorMessages.GetSummaryFailed,
                Succeeded = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Data = null,
                Errors = new[] { ex.Message }
            };

        }
    }
}




