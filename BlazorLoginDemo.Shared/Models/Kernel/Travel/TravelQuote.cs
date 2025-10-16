// Models/Kernel/Travel/TravelQuote.cs
using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;

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

    // Travellers
    public ICollection<TravelQuoteUser> Travellers { get; set; } = new List<TravelQuoteUser>();
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

public enum QuoteState
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Expired = 5
}

public sealed class TravelQuoteDto
{
    public string QuoteType { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string TmcAssignedId { get; set; } = null!;
    public string OrganizationId { get; set; } = null!;
    public List<string> TravellerUserIds { get; set; } = new();
}