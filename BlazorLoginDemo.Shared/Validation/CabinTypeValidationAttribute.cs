using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Validation;

public class CabinTypeValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> _validValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "ECONOMY", "PREMIUM_ECONOMY", "BUSINESS", "FIRST"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string strValue && _validValues.Contains(strValue))
        {
            return ValidationResult.Success!; // Null-forgiving operator removes the warning
        }
        return new ValidationResult($"Invalid cabin type. Allowed values: {string.Join(", ", _validValues)}");
    }
}
