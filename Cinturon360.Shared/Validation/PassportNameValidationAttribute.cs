using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Cinturon360.Shared.Validation;

/// <summary>
/// Validates that a passport name (printed version) contains only allowed characters.
/// Allowed characters:
/// - Unicode letters (including accented letters)
/// - Combining diacritical marks
/// - Whitespace
/// - Hyphens (-)
/// - Apostrophes (')
/// </summary>
public class PassportNameValidationAttribute : ValidationAttribute
{
    // Explanation of the pattern:
    // \p{L} matches any Unicode letter.
    // \p{M} matches any diacritical mark (for decomposed accented characters).
    // \s matches whitespace characters.
    // The literal characters - and ' are also allowed.
    // The pattern ensures that the entire string consists of one or more of these allowed characters.
    private const string Pattern = @"^[\p{L}\p{M}\s'-]+$";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Allow null values; add a [Required] attribute if nulls should be disallowed.
        if (value == null)
        {
            return ValidationResult.Success;
        }

        string strValue = value.ToString() ?? string.Empty;
        if (Regex.IsMatch(strValue, Pattern))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("The field contains invalid characters. Only letters, spaces, hyphens, and apostrophes are allowed.");
    }
}
