using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Validation;

public class PaymentMethodValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> _validValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Account0", "Account30", "Account60", "Account90", "Stripe", "PayPal", "BankTransfer", "Trial90", "Trial120",
        "UAT", "Gratis"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string strValue && _validValues.Contains(strValue))
        {
            return ValidationResult.Success!; // Null-forgiving operator removes the warning
        }
        return new ValidationResult($"Invalid payment method. Allowed values: {string.Join(", ", _validValues)}");
    }
}
