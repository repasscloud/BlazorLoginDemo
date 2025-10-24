namespace BlazorLoginDemo.Shared.Services.Interfaces.Persistence;

using BlazorLoginDemo.Shared.Models.Kernel.FX;

public interface IFxRateStore
{
    Task<ExchangeRateSnapshot?> GetLatestAsync(string baseCode, CancellationToken ct = default);
    Task<Guid> SaveAsync(ExchangeRateSnapshot snapshot, CancellationToken ct = default);
}