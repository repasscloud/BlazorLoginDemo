using System.ComponentModel.DataAnnotations;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Search;

public sealed class PnrSearchRecord
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();

    public string PnrCode { get; set; } = string.Empty;

    public string BookingUserId { get; set; } = string.Empty;
    
    

    public string TenantName { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string TravelQuoteId { get; set; } = string.Empty;
    public string TmcAssignedId { get; set; } = string.Empty;
    
}