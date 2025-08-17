namespace BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;

public class AmadeusSettings
{
    public AmadeusUrlSettings Url { get; set; } = new AmadeusUrlSettings();
}

public class AmadeusUrlSettings
{
    public string FlightOffer { get; set; } = string.Empty;
}
