namespace BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;

public class AmadeusOAuthClientSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public AmadeusUrlSettings Url { get; set; } = new();
}
