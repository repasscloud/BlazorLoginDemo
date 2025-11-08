using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Validation;

/// <summary>
/// Applies standard markup precision of (5,4) for percentage fields.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MarkupPrecisionAttribute : PrecisionAttribute
{
    public MarkupPrecisionAttribute() : base(5, 4)
    {
    }
}
