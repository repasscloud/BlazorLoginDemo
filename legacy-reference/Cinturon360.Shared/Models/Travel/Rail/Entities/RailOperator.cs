using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Travel.Rail.Enums;
using Cinturon360.Shared.Helpers;
using Cinturon360.Shared.Validation;

namespace Cinturon360.Shared.Models.Travel.Rail.Entities;

/// <summary>
/// Master rail operator record.
/// Global-ready for EU / AU / APAC.
/// Designed for corporate travel, distribution integrations, compliance, and booking capability modelling.
/// </summary>
public sealed record RailOperator
{
    // =====================================================================
    // PLATFORM IDENTITY
    // =====================================================================

    /// <summary>
    /// Platform canonical ID (example: rop_xxxxx).
    /// This is the ONLY guaranteed unique identity globally.
    /// </summary>
    [Key]
    [MaxLength(20)]
    public string Id { get; init; } = IDGeneratorHelper.GenerateId(IdGenType.RailOperator);

    /// <summary>
    /// Public or trading operator name.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Legal registered entity name (contracts / compliance / invoicing).
    /// </summary>
    public string? LegalName { get; init; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (AU, FR, JP, etc).
    /// </summary>
    [Alpha2Validation]
    public string CountryIso2 { get; init; } = default!;

    /// <summary>
    /// Primary operator classification.
    /// </summary>
    public RailOperatorType OperatorType { get; init; } = RailOperatorType.Passenger;


    // =====================================================================
    // BOOKING + RETAIL CAPABILITIES
    // =====================================================================

    /// <summary>Can issue tickets via API / distribution.</summary>
    public bool SupportsTicketing { get; init; } = false;

    /// <summary>Supports seat selection / reserved seating.</summary>
    public bool SupportsSeatReservation { get; init; } = false;

    /// <summary>Uses yield / dynamic pricing.</summary>
    public bool SupportsDynamicPricing { get; init; } = false;

    /// <summary>National ID or passport required at booking time.</summary>
    public bool RequiresNationalId { get; init; } = false;

    /// <summary>Supports electronic ticket formats (PDF / barcode / QR).</summary>
    public bool SupportsETicket { get; init; } = false;

    /// <summary>Supports mobile wallet or native mobile ticketing.</summary>
    public bool SupportsMobileTicket { get; init; } = false;

    /// <summary>Supports negotiated corporate travel contracts.</summary>
    public bool SupportsCorporateAccounts { get; init; } = false;

    /// <summary>Has customer loyalty / rewards scheme.</summary>
    public bool SupportsLoyaltyProgram { get; init; } = false;

    /// <summary>Supports refunds automation.</summary>
    public bool SupportsRefunds { get; init; } = false;

    /// <summary>Supports ticket exchanges.</summary>
    public bool SupportsExchanges { get; init; } = false;

    /// <summary>Supports multi-operator through ticketing.</summary>
    public bool SupportsInterlineTicketing { get; init; } = false;

    /// <summary>Supports group reservations.</summary>
    public bool SupportsGroupBookings { get; init; } = false;

    /// <summary>Seat map data available via API.</summary>
    public bool SupportsSeatMaps { get; init; } = false;


    // =====================================================================
    // DATA + API CAPABILITIES
    // =====================================================================

    /// <summary>Realtime delay / disruption feeds available.</summary>
    public bool SupportsRealtimeStatus { get; init; } = false;

    /// <summary>Timetable / schedule API exists.</summary>
    public bool SupportsTimetableApi { get; init; } = false;

    /// <summary>Fare pricing API exists.</summary>
    public bool SupportsFareApi { get; init; } = false;

    /// <summary>Availability / inventory API exists.</summary>
    public bool SupportsAvailabilityApi { get; init; } = false;


    // =====================================================================
    // INDUSTRY IDENTIFIERS
    // =====================================================================

    /// <summary>Member of UIC (mostly EU relevance).</summary>
    public bool IsUicMember { get; init; } = false;

    /// <summary>UIC company code.</summary>
    public string? UicCode { get; init; }

    /// <summary>Railway Interchange Coding System identifier (if used).</summary>
    public string? RicsCode { get; init; }

    /// <summary>Vehicle Keeper Mark (rolling stock owner reference).</summary>
    public string? VkmCode { get; init; }


    // =====================================================================
    // REGULATORY / SAFETY
    // =====================================================================

    /// <summary>National rail regulator accreditation or licence ID.</summary>
    public string? LocalRegulatorId { get; init; }

    /// <summary>Safety certification reference.</summary>
    public string? SafetyCertificationId { get; init; }


    // =====================================================================
    // AUSTRALIA CORPORATE IDENTIFIERS
    // =====================================================================

    /// <summary>Australian Business Number (11 digits).</summary>
    [AuAbnValidation]
    public string? Abn { get; init; }

    /// <summary>Australian Company Number (9 digits).</summary>
    [AuAcnValidation]
    public string? Acn { get; init; }


    // =====================================================================
    // GLOBAL CORPORATE IDENTIFIERS
    // =====================================================================

    /// <summary>Dun & Bradstreet DUNS number.</summary>
    public string? DunsNumber { get; init; }

    /// <summary>Legal Entity Identifier (financial / regulatory reporting).</summary>
    public string? LeiCode { get; init; }

    /// <summary>VAT / GST / tax registration reference.</summary>
    public string? TaxRegistrationNumber { get; init; }


    // =====================================================================
    // DISTRIBUTION / CONTENT ECOSYSTEM
    // =====================================================================

    /// <summary>
    /// Primary content distribution source.
    /// </summary>
    public RailDistributionProvider? DistributionProvider { get; init; }

    /// <summary>
    /// Operator code inside that distribution ecosystem.
    /// </summary>
    public string? DistributionOperatorCode { get; init; }

    /// <summary>
    /// Direct retail / partner API endpoint if applicable.
    /// </summary>
    public string? DirectApiEndpoint { get; init; }


    // =====================================================================
    // OPERATIONAL MODES
    // =====================================================================

    public bool IsPassenger { get; init; }
    public bool IsFreight { get; init; }
    public bool IsInfrastructureManager { get; init; }


    // =====================================================================
    // COMMERCIAL / BILLING
    // =====================================================================

    /// <summary>Supports prepaid settlement models.</summary>
    public bool SupportsPrepaidAccounts { get; init; }

    /// <summary>Supports credit billing / invoicing.</summary>
    public bool SupportsCreditBilling { get; init; }

    /// <summary>Supports agency / reseller commission models.</summary>
    public bool SupportsCommission { get; init; }


    // =====================================================================
    // CONTACT / DISCOVERY
    // =====================================================================

    public string? WebsiteUrl { get; init; }
    public string? SupportEmail { get; init; }
    public string? SupportPhone { get; init; }


    // =====================================================================
    // LIFECYCLE
    // =====================================================================

    public bool IsActive { get; init; } = true;

    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; init; }
}
