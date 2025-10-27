// Models/Kernel/Travel/TravelQuote.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Search;

namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class TravelQuote
{
    [Key, MaxLength(64)]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate();

    public TravelQuoteType Type { get; set; } = TravelQuoteType.Unknown;

    public QuoteState State { get; set; } = QuoteState.Draft;

    // Client organization
    [Required, MaxLength(64)]
    public string OrganizationId { get; set; } = null!;
    public OrganizationUnified Organization { get; set; } = null!;

    // Who created this quote
    [Required, MaxLength(64)]
    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;

    // Which TMC this quote is assigned to
    [Required, MaxLength(64)]
    public string TmcAssignedId { get; set; } = null!;
    public OrganizationUnified TmcAssigned { get; set; } = null!;

    // Ephemeral travel policy is stored in a separate table that was introduced in issue 
    public TravelQuotePolicyType PolicyType { get; set; } = TravelQuotePolicyType.Unknown;

    [MaxLength(14)]
    public string TravelPolicyId { get; set; } = null!;  // (invisible) FK to TravelPolicy.Id

    // Currency (#68)
    [MaxLength(3)]
    public string Currency { get; set; } = "AUD";

    // note: optional internal note about this quote
    [MaxLength(4096)]
    public string? Note { get; set; }

    // approvals managed by system workflows (workflows will always use the lowest common denominator)
    public bool ApprovalLevel0 { get; set; } = false;
    public bool ApprovalLevel1 { get; set; } = false;
    public bool ApprovalLevel2 { get; set; } = false;
    public bool ApprovalLevel3 { get; set; } = false;
    public bool ApprovalLevel4 { get; set; } = false;
    public bool ApprovalLevel5 { get; set; } = false;

    // Timestamp
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Travellers
    public ICollection<TravelQuoteUser> Travellers { get; set; } = new List<TravelQuoteUser>();

    // Customer Query Data (FLIGHT)
    public TripType? TripType { get; set; }
    [MaxLength(3)] public string? OriginIataCode { get; set; }
    [MaxLength(3)] public string? DestinationIataCode { get; set; }
    [MaxLength(10)] public string? DepartureDate { get; set; }
    [MaxLength(10)] public string? ReturnDate { get; set; }
    [MaxLength(8)] public string? DepartEarliestTime { get; set; }
    [MaxLength(8)] public string? DepartLatestTime { get; set; }
    [MaxLength(8)] public string? ReturnEarliestTime { get; set; }
    [MaxLength(8)] public string? ReturnLatestTime { get; set; }
    public CoverageType? CoverageType { get; set; }
    public CabinClass? CabinClass { get; set; }
    public CabinClass? MaxCabinClass { get; set; }
    public string[] SelectedAirlines { get; set; } = Array.Empty<string>();
    public List<Alliance>? Alliances { get; set; }
    
    // Helper properties for CSV serialization
    [NotMapped]
    public string SelectedAirlinesCsv
    {
        get => string.Join(", ", SelectedAirlines);
        set => SelectedAirlines = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
}

public sealed class TravelQuoteUser
{
    [Key, MaxLength(64)]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate();

    [Required, MaxLength(64)]
    public string TravelQuoteId { get; set; } = null!;
    public TravelQuote TravelQuote { get; set; } = null!;

    [Required, MaxLength(64)]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public enum TravelQuoteType
{
    Unknown = 0,
    flight = 1,
    accomodation = 2,
    taxi = 3,
    train = 4,
    hirecar = 5,
    bus = 6,
    simcard = 7,
    activity = 8,
    mixed = 9
}

public enum QuoteState : short
{
    // 00–09: Drafting / local-only UI states
    Draft = 00,
    RetrievedUI = 10,            // UI loaded a previously-saved quote for editing/view
    SearchResultsRequested = 11, // UI triggered a search to build a quote

    // 20–39: Submission lifecycle
    Submitted = 20,

    // 40–59: Decision outcomes
    Approved = 40,
    Rejected = 41,

    // 60–79: Terminations and timeouts
    Cancelled = 60,
    Expired = 61,

    // 90–99: Post-lifecycle storage
    Archived = 90
}

public sealed class TravelQuoteDto
{
    public string QuoteType { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string TmcAssignedId { get; set; } = null!;
    public string OrganizationId { get; set; } = null!;
    public List<string> TravellerUserIds { get; set; } = new();
}

public enum TravelQuotePolicyType
{
    Unknown = -1,
    OrgDefault = 0,
    UserDefined = 1,
    Ephemeral = 2
}