namespace ExpenseTracker.Models.Validations
{

    public static class ValidationMessages
    {

        public const string CategoryLettersOnly = "Category must contain only letters and cannot be just spaces.";
        public const string PositiveAmount = "Amount must be a positive number greater than 0.";
        public const string NoteCannotBeSpaces = "Note cannot be only spaces.";
        public const string FutureDateNotAllowed = "Future dates are not allowed for expenses.";
        public const string NameOnlyLetters = "Name must contain only letters and cannot be just spaces.";
        public const string DescriptionCannotBeSpaces = "Description cannot be only spaces.";
        public const string InvalidEmail = "Invalid email address.";
        public const string EmailSpecialChar = "Email cannot start with a special character.";
        public const string PasswordNoSpaces = "Password cannot contain spaces.";
        public const string NoSpacesUsername = "Username cannot contain spaces.";
        public const string PasswordMinLength = "Password must be at least 8 characters long.";
        public const string PasswordRequirements = "Password must be at least 8 characters, include uppercase, lowercase, number, special character, and have no spaces.";
        public const string FirstNameOnlyLetters = "FirstName must contain only letters and cannot be just spaces.";
        public const string LastNameOnlyLetters = "LastName must contain only letters and cannot be just spaces.";
        public const string ContactnumberFormat = "Contact Number must be exactly 10 digits and cannot start with 0.";

    }

    public static class RequiredValidationMessages
    {
        public const string EmailRequired = "Email is required.";
        public const string UsernameRequired = "Username is required.";
        public const string PasswordRequired = "Password is required.";
        public const string FirstNameRequired = "FirstName is required.";
        public const string LastNameRequired = "LastName is required.";
        public const string PhoneRequired = "Phone is required.";
        public const string CategoryRequired = "Category Name is required.";
        public const string DescriptionRequired = "Description is required.";
        public const string NoteRequired = "Note is required.";

    }

}