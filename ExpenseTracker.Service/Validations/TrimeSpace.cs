using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Filters;
namespace ExpenseTracker.Service.Validations
{
    public class TrimAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var parameters = filterContext.ActionArguments;
            foreach (var param in parameters)
            {
                TrimStringProperties(param.Value);
            }

            base.OnActionExecuting(filterContext);
        }

        private static void TrimStringProperties(object obj)
        {
            if (obj == null) return;

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(string) && prop.CanWrite)
                {
                    string currentValue = (string)prop.GetValue(obj);
                    if (currentValue != null)
                    {
                        prop.SetValue(obj, currentValue.Trim());
                    }
                }

            }
        }
    }

    public class NoWhiteSpaceOnly : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true; // Let Required attribute handle null values
            }

            string stringValue = value.ToString();
            return !string.IsNullOrEmpty(stringValue.Trim());
        }
    }

}