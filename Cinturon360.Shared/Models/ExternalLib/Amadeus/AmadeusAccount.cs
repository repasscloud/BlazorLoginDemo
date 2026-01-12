using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public sealed class AmadeusAccount
{
    // Tenant scope (matches TMC primary key exactly)
    [Required]
    [MaxLength(64)]
    public string TmcId { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; init; } = null!;

    // OAuth
    [Required]
    [MaxLength(256)]
    public string ClientId { get; init; } = null!;

    [Required]
    [MaxLength(512)]
    public string ClientSecret { get; init; } = null!;

    // Commercial Identity
    [Required]
    [MaxLength(32)]
    public string OfficeId { get; init; } = null!;

    [Required]
    [MaxLength(2)]
    public string CountryCode { get; init; } = null!;

    [Required]
    [MaxLength(3)]
    public string DefaultCurrency { get; init; } = null!;

    // Capabilities
    public bool TicketingEnabled { get; init; }

    [MaxLength(3)]
    public string? DefaultPlatingCarrier { get; init; }

    [MaxLength(3)]
    public string? TicketPrefix { get; init; }

    // URLs (environment-specific)
    [Required]
    public AmadeusUrlSettings Url { get; init; } = new();
}
