namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public sealed class AmadeusOAuthClientContract
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;

    public AmadeusUrlSettings Url { get; init; } = new();
}
