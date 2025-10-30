using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public class AmadeusOAuthToken
{
    [Key]
    public long Id { get; set; }
    public string TokenType { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }  // Stores 1799 seconds
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryTime => CreatedAt.AddSeconds(ExpiresIn); // Token expiry timestamp
}
