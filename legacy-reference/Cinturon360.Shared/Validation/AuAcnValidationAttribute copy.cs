using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Cinturon360.Shared.Validation;

public class AuAcnValidationAttribute : ValidationAttribute
{
    private const string Pattern = "^[0-9]{9}$";
    
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
        
        return new ValidationResult("The field must be exactly 9 numeric characters (0-9).");
    }
}
