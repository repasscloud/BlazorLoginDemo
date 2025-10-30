using Cinturon360.Shared.Models.ExternalLib.Amadeus;

namespace Cinturon360.Shared.Services.Interfaces.External;

public interface IAmadeusAuthService
{
    Task<AmadeusOAuthToken> GetTokenAsync();

    Task<string> GetTokenInformationAsync();  // returns the actual token as a string, do not call the line above
}
