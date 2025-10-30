using System.ComponentModel.DataAnnotations;
using NanoidDotNet;

namespace Cinturon360.Shared.Models.Kernel.Client;

public class AvaClientLicense
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();
    
    [Required]
    public required string ClientID { get; set; }

    [Required]
    public required DateTime ExpiryDate { get; set; }

    [Required]
    [MaxLength(20)]
    public required string AppID { get; set; }

    public string? Signature { get; set; }
    
    [Required]
    public decimal SpendThreshold { get; set; } // what is the spend threshold of this particular client

    [Required]
    public required string IssuedBy { get; set; }

    public DateTime GeneratedOn { get; set; } = DateTime.UtcNow; // when the license was generated
}
