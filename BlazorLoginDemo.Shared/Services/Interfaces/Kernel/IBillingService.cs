// File: Shared/Services/Interfaces/Kernel/IBillingService.cs
using System.Linq.Expressions;
using BlazorLoginDemo.Shared.Models.Kernel.Billing;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Kernel;

public interface IBillingService
{
    // -------- Discounts: DTOs / Params --------
    public sealed record CreateDiscountRequest(
        string DisplayName,
        DiscountType Type,
        decimal Amount,
        DiscountScope Scope,
        string? Currency            = null,
        string? ParentOrgId         = null,
        string? DiscountCode        = null,   // if null, system generates
        int? DurationInPeriods      = null,
        DateTime? StartsAtUtc       = null,
        DateTime? EndsAtUtc         = null,
        int? SeatMin                = null,
        int? SeatMax                = null,
        bool IsEnabled              = true,
        string? Notes               = null
    );

    public sealed record CreateDiscountResult(bool Ok, string? Error, string? DiscountId);

    public sealed record DiscountSearchParams(
        string? Text                 = null,     // matches DisplayName or Code
        string? ParentOrgId          = null,
        bool? IsEnabled              = null,
        bool? ActiveNow              = null,     // IsActive computed window
        DiscountType? Type           = null,
        int? ScopeMinInclusive       = null,     // e.g., 100
        int? ScopeMaxInclusive       = null,     // e.g., 199
        DiscountScope? ScopeEquals   = null,
        DateTime? StartsBeforeUtc    = null,
        DateTime? EndsAfterUtc       = null,
        int? Skip                    = 0,
        int? Take                    = 50
    );

    public sealed record DiscountPickerDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? Code { get; init; }
        public DiscountType Type { get; init; }
        public decimal Amount { get; init; }
        public DiscountScope Scope { get; init; }
        public bool IsEnabled { get; init; }
        public bool IsActive { get; init; }
        public string? Currency { get; init; }
        public DateTime? StartsAtUtc { get; init; }
        public DateTime? EndsAtUtc { get; init; }
    }

    // --------------- CREATE / UPSERT ---------------
    Task<Discount> CreateDiscountAsync(CreateDiscountRequest req, CancellationToken ct = default);
    Task<CreateDiscountResult> CreateDiscountSimpleAsync(CreateDiscountRequest req, CancellationToken ct = default);
    Task<Discount> UpsertDiscountByCodeAsync(string code, CreateDiscountRequest req, CancellationToken ct = default);

    // ------------------- READ ----------------------
    Task<Discount?> GetDiscountByIdAsync(string id, CancellationToken ct = default);
    Task<Discount?> GetDiscountByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Discount>> SearchDiscountsAsync(DiscountSearchParams filters, CancellationToken ct = default);
    Task<IReadOnlyList<DiscountPickerDto>> GetDiscountsForPickerAsync(DiscountSearchParams filters, CancellationToken ct = default);

    // ------------------ UPDATE ---------------------
    /// <summary>Whole-object replace: updates scalar fields only, ignores navs.</summary>
    Task<bool> UpdateDiscountAsync(Discount replacement, CancellationToken ct = default);

    /// <summary>Single-field update. Example: UpdateDiscountFieldAsync(id, d => d.IsEnabled, false)</summary>
    Task<bool> UpdateDiscountFieldAsync<T>(string id, Expression<Func<Discount, T>> property, T value, CancellationToken ct = default);

    /// <summary>Enable/Disable without changing other fields.</summary>
    Task<bool> SetDiscountEnabledAsync(string id, bool enabled, CancellationToken ct = default);

    // ----------------- DELETE ----------------------
    Task<bool> DeleteDiscountAsync(string id, CancellationToken ct = default);
}
