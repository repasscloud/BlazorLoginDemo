using Cinturon360.Shared.Models.ExternalLib.Amadeus;

namespace Cinturon360.Shared.Services.Interfaces.External;

public interface IAmadeusAccountService
{
    Task<AmadeusAccount> CreateAsync(AmadeusAccount account, CancellationToken ct = default);

    Task<AmadeusAccount> UpdateAsync(AmadeusAccount account, CancellationToken ct = default);

    Task<AmadeusAccount?> GetAsync(string tmcId, CancellationToken ct = default);

    Task<bool> ExistsAsync(string tmcId, CancellationToken ct = default);
}
