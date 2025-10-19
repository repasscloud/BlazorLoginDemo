using BlazorLoginDemo.Shared.Models.Kernel.FX;
namespace BlazorLoginDemo.Shared.Services.Interfaces.External;
public interface IFxRateService
{
    Task<ExchangeRateResponse> GetRatesAsync(string baseCode, CancellationToken ct = default);
    Task<decimal> GetRateAsync(string from, string to, CancellationToken ct = default);
    Task<ConvertResult> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default);

    Task<decimal?> GetLatestSnapshotRateAsync(string baseCode, string quoteCode, CancellationToken ct = default);
}