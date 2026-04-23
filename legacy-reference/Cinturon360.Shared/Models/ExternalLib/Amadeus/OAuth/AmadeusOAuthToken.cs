using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public class AmadeusOAuthToken
{
    [Key]
    public long Id { get; set; }

    // Tenant scope (matches TMC Id type exactly)
    [Required]
    [MaxLength(64)]
    public string TmcId { get; set; } = null!;

    [Required]
    [MaxLength(32)]
    public string TokenType { get; set; } = null!;

    [Required]
    public string AccessToken { get; set; } = null!;

    // Typically 1799 seconds
    public int ExpiresIn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryTime => CreatedAt.AddSeconds(ExpiresIn);
}
