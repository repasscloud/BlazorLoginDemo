namespace Cinturon360.Shared.Services.Interfaces.External;

using Cinturon360.Shared.Models.ExternalLib.Amadeus;

public interface IAmadeusAccountStore
{
    Task<AmadeusAccount> GetByTmcIdAsync(string tmcId);
}
