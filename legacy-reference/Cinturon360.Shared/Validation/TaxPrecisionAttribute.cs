using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Validation;

/// <summary>
/// Applies standard tax precision of (5,4) for tax rates.
/// Used for tax fields like 0.1000 (10%) or 0.0750 (7.5%).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TaxPrecisionAttribute : PrecisionAttribute
{
    public TaxPrecisionAttribute() : base(6, 4)
    {
    }
}
