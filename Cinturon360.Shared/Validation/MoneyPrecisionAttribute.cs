using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Validation;

/// <summary>
/// Applies standard money precision of (18,2) for decimal fields.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MoneyPrecisionAttribute : PrecisionAttribute
{
    public MoneyPrecisionAttribute() : base(18, 2)
    {
    }
}
