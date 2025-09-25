using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;

namespace BlazorLoginDemo.Shared.Services.Interfaces.External;

public interface IAmadeusAuthService
{
    Task<AmadeusOAuthToken> GetTokenAsync();

    Task<string> GetTokenInformationAsync();  // returns the actual token as a string, do not call the line above
}
