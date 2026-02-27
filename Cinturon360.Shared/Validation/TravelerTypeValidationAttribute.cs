using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Validation;

public class TravelerTypeValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> _validValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "ADULT", "CHILD", "SENIOR", "YOUNG", "HELD_INFANT", "SEATED_INFANT", "STUDENT"
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
