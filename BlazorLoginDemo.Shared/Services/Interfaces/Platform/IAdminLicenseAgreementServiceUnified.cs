using System.Linq.Expressions;
using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Static.Billing;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Platform;

// ======================================
// LICENSE AGREEMENT (Unified) - Interface
// ======================================
public interface IAdminLicenseAgreementServiceUnified
{
    // -------- Aggregates / DTOs --------
    public sealed record LicenseAggregate(
        LicenseAgreementUnified License,
        OrganizationUnified Organization,            // OrganizationUnifiedId (the licensee)
        OrganizationUnified Issuer                   // CreatedByOrganizationUnifiedId (the issuer)
    );

    public sealed record CreateRequest(
        string OrganizationUnifiedId,
        string CreatedByOrganizationUnifiedId,
        LicenseAgreementUnified? Model               // optional: if null, create with sensible defaults
    );

    public sealed record CreateResult(bool Ok, string? Error, string? LicenseAgreementId);

    public sealed record SearchParams(
        string? OrganizationId        = null,
        string? IssuerOrganizationId  = null,
        DateOnly? StartsOnOrAfter     = null,
        DateOnly? ExpiresOnOrBefore   = null,
        bool? AutoRenew               = null,
        decimal? MinPrepaidBalanceGte = null,
        decimal? MinMonthlySpendGte   = null,
        PaymentStatus? PaymentStatus  = null
    );

    // --------------- CREATE / UPSERT ---------------
    Task<LicenseAggregate> CreateAsync(CreateRequest req, CancellationToken ct = default);
    Task<CreateResult> CreateLicenseAsync(CreateRequest req, CancellationToken ct = default);

    /// <summary>
    /// If org has a license, update it (using the provided Model). Otherwise create one.
    /// Validates OrganizationUnifiedId and CreatedByOrganizationUnifiedId.
    /// </summary>
    Task<LicenseAggregate> UpsertForOrganizationAsync(
        string organizationUnifiedId,
        string createdByOrganizationUnifiedId,
        LicenseAgreementUnified model,
        CancellationToken ct = default);

    // ------------------- READ ----------------------
    Task<LicenseAggregate?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<LicenseAggregate?> GetByOrganizationIdAsync(string organizationUnifiedId, CancellationToken ct = default);
    Task<IReadOnlyList<LicenseAggregate>> SearchAsync(SearchParams filters, CancellationToken ct = default);

    /// <summary>Simple picker list for UI. Ordered by Org.Name then Id.</summary>
    public sealed class PickerDto
    {
        public required string Id { get; init; }
        public required string OrganizationId { get; init; }
        public required string OrganizationName { get; init; }
        public required string IssuerOrganizationId { get; init; }
        public required string IssuerOrganizationName { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly ExpiryDate { get; init; }
        public bool AutoRenew { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal PrepaidBalance { get; init; }
    }
    Task<IReadOnlyList<PickerDto>> GetAllForPickerAsync(CancellationToken ct = default);

    // ------------------ UPDATE ---------------------
    /// <summary>
    /// Whole-object “replace” semantics: attach root, mark only scalar/owned props modified, ignore navs.
    /// Validates org links if changed.
    /// </summary>
    Task<bool> UpdateAsync(LicenseAgreementUnified replacement, CancellationToken ct = default);

    /// <summary>
    /// Strongly-typed single-field update (no tracking; single SQL UPDATE).
    /// Example: await UpdateFieldAsync(id, x => x.PaymentStatus, PaymentStatus.Active)
    /// </summary>
    Task<bool> UpdateFieldAsync<T>(
        string id,
        Expression<Func<LicenseAgreementUnified, T>> property,
        T value,
        CancellationToken ct = default);

    /// <summary>
    /// Reassign license to a different org and/or issuer (validates existence).
    /// </summary>
    Task<bool> ReassignOrganizationsAsync(
        string id,
        string newOrganizationUnifiedId,
        string newCreatedByOrganizationUnifiedId,
        CancellationToken ct = default);

    /// <summary>
    /// Atomic increment/decrement of PrepaidBalance (server-side += delta).
    /// </summary>
    Task<bool> AdjustPrepaidBalanceAsync(string id, decimal delta, CancellationToken ct = default);

    // ----------------- DELETE ----------------------
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
