using Cinturon360.Shared.Models.ExternalLib.Amadeus;

namespace Cinturon360.Shared.Services.Interfaces.External;

public interface IAmadeusAuthService
{
    Task<AmadeusOAuthToken> GetTokenAsync(string tmcId);

    Task<string> GetAccessTokenAsync(string tmcId);
}
