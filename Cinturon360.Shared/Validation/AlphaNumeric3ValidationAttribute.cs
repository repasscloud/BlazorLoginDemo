using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Cinturon360.Shared.Validation;

public class AlphaNumeric3ValidationAttribute : ValidationAttribute
{
    private const string Pattern = "^[A-Za-z0-9]{3}$";
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Consider null valid; use [Required] if you want non-null values.
        if (value == null)
        {
            return ValidationResult.Success;
        }
        
        string strValue = value.ToString() ?? string.Empty;
        if (Regex.IsMatch(strValue, Pattern))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult("The field must be exactly 3 alphanumeric characters (A-Z, 0-9).");
    }
}
