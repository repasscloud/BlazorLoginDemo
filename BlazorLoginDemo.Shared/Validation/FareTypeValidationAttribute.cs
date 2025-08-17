using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Validation;

public class FareTypeValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> _validValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "STANDARD", "INCLUSIVE_TOUR", "SPANISH_MELILLA_RESIDENT", "SPANISH_CEUTA_RESIDENT", "SPANISH_CANARY_RESIDENT", 
        "SPANISH_BALEARIC_RESIDENT", "AIR_FRANCE_METROPOLITAN_DISCOUNT_PASS", "AIR_FRANCE_DOM_DISCOUNT_PASS",
        "AIR_FRANCE_COMBINED_DISCOUNT_PASS", "AIR_FRANCE_FAMILY", "ADULT_WITH_COMPANION", "COMPANION"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string strValue && _validValues.Contains(strValue))
        {
            return ValidationResult.Success!; // Null-forgiving operator removes the warning
        }
        return new ValidationResult($"Invalid fare type. Allowed values: {string.Join(", ", _validValues)}");
    }
}
