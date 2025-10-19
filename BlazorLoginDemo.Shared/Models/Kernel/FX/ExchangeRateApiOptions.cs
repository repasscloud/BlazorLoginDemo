namespace BlazorLoginDemo.Shared.Models.Kernel.FX;

public sealed class ExchangeRateApiOptions
{
    public string BaseUrl { get; set; } = "https://v6.exchangerate-api.com/v6";
    public string ApiKey { get; set; } = "";
    public string DefaultBaseCode { get; set; } = "USD";
}