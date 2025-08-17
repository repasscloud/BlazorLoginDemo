namespace BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;

public class AmadeusOAuthToken
{
    public int Id { get; set; }
    public string TokenType { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }  // Stores 1799 seconds
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryTime => CreatedAt.AddSeconds(ExpiresIn); // Token expiry timestamp
}
