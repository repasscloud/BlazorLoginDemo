using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Validation;

public class CoverageTypeValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> _validValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "MOST_SEGMENTS", "AT_LEAST_ONE_SEGMENT", "ALL_SEGMENTS"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string strValue && _validValues.Contains(strValue))
        {
            return ValidationResult.Success!; // Null-forgiving operator removes the warning
        }
        return new ValidationResult($"Invalid traveler type. Allowed values: {string.Join(", ", _validValues)}");
    }
}
