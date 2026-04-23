using Cinturon360.Shared.Models.ExternalLib.Amadeus;

namespace Cinturon360.Shared.Services.Interfaces.External;

public interface IAmadeusConnectionTestService
{
    Task<AmadeusConnectionTestResult> TestAsync(
        string tmcId,
        CancellationToken ct = default);
}
