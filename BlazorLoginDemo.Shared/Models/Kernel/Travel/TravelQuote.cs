// Models/Kernel/Travel/TravelQuote.cs
using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;

namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class TravelQuote
{
    [Key, MaxLength(64)]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate();

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
