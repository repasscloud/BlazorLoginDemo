using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Kernel.UI.Amadeus;

public sealed class AmadeusAccountEditModel
{
    // Tenant
    [Required]
    [MaxLength(64)]
    public string TmcId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    // OAuth
    [Required]
    [MaxLength(128)]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string ClientSecret { get; set; } = string.Empty;

    // Commercial Identity
    [Required]
    [MaxLength(32)]
    public string OfficeId { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(3)]
    public string DefaultCurrency { get; set; } = string.Empty;

    // Capabilities
    public bool TicketingEnabled { get; set; }

    [MaxLength(3)]
    public string? DefaultPlatingCarrier { get; set; }

    [MaxLength(3)]
    public string? TicketPrefix { get; set; }

    // Environment / URLs
    [Required]
    public string ApiEndpoint { get; set; } = string.Empty;

    public bool ApiEndpointValidated { get; set; } = false;
}
