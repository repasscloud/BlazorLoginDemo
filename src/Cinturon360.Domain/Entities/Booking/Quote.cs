using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Booking;

namespace Cinturon360.Domain.Entities.Booking;

/// <summary>
/// A travel quote (price offer) that can be booked. Expires after a set period.
/// </summary>
public sealed class Quote : SoftDeletableEntity
{
    public string OrgId { get; private set; } = string.Empty;
    public string RequestedByUserId { get; private set; } = string.Empty;
    public QuoteStatus Status { get; private set; } = QuoteStatus.Active;

    public decimal TotalAmountGross { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public DateTimeOffset ExpiresAt { get; private set; }

    // Provider snapshot (JSON blob, provider-specific)
    public string? ProviderPayload { get; private set; }
    public string? ProviderRef { get; private set; }

    private Quote() { }

    public static Quote Create(
        string id,
        string orgId,
        string requestedByUserId,
        decimal totalAmount,
        string currencyCode,
        DateTimeOffset expiresAt,
        string? providerRef = null)
        => new()
        {
            Id = id,
            OrgId = orgId,
            RequestedByUserId = requestedByUserId,
            TotalAmountGross = totalAmount,
            CurrencyCode = currencyCode,
            ExpiresAt = expiresAt,
            ProviderRef = providerRef,
            Status = QuoteStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public bool IsExpired() => DateTimeOffset.UtcNow > ExpiresAt;

    public void MarkBooked() { Status = QuoteStatus.Booked; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkExpired() { Status = QuoteStatus.Expired; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkAbandoned() { Status = QuoteStatus.Abandoned; UpdatedAt = DateTimeOffset.UtcNow; }

    public void SetProviderPayload(string payload)
    {
        ProviderPayload = payload;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
