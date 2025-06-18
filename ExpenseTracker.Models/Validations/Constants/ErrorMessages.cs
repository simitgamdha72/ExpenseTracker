namespace ExpenseTracker.Models.Validations.Constants.ErrorMessages
{
    public static class ErrorMessages
    {
        public const string EmailOrUsernameExists = "Email or Username already exists.";
        public const string RegistrationFailed = "An error occurred during registration.";
        public const string InvalidCredentials = "Invalid credentials";
        public const string LoginFailed = "An error occurred while logging in.";
        public const string StartDateInFuture = "Start date cannot be in the future.";
        public const string EndDateInFuture = "End date cannot be in the future.";
        public const string StartDateAfterEndDate = "Start date cannot be greater than end date.";
        public const string StartMonthInFuture = "Start month cannot be in the future.";
        public const string EndMonthInFuture = "End month cannot be in the future.";
        public const string StartMonthAfterEndMonth = "Start month cannot be greater than end month.";
        public const string CustomMonthRangeRequired = "Start and end month/year must be provided for custom monthly reports.";
        public const string CategoryNotFound = "Category not found.";
        public const string CategoryNameExists = "Category name already exists!";
        public const string InvalidCategory = "Invalid category.";
        public const string UserNotFound = "User not found!";
        public const string ExpenseNotFound = "Expense not found.";
        public const string GetExpensesFailed = "An error occurred while retrieving expenses.";
        public const string CreateExpenseFailed = "An error occurred while creating the expense.";
        public const string UpdateExpenseFailed = "An error occurred while updating the expense.";
        public const string DeleteExpenseFailed = "An error occurred while deleting the expense.";
        public const string FutureDateNotAllowed = "Dates cannot be in the future.";
        public const string FutureMonthNotAllowed = "Months cannot be in the future.";
        public const string GetSummaryFailed = "An error occurred while retrieving expense summary.";
        public const string ExportCsvFailed = "An error occurred while exporting expenses.";
        public const string InternalServerError = "An Internal server error occurred. Please try again later.";
        public const string NotFound = "Not found";
        public const string InvalidRole = "Invalid role. Role must be 1 or 2.";
        public const string UnauthorizedAccess = "Unauthorized access. Please log in.";

    }
}
