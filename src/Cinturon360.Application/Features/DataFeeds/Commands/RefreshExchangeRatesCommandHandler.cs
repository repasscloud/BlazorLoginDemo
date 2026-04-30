using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Pricing;
using Cinturon360.Integrations.ExchangeRates;

namespace Cinturon360.Application.Features.DataFeeds.Commands;

public sealed class RefreshExchangeRatesCommandHandler(
    IExchangeRateProvider provider,
    IExchangeRateRepository repository,
    IUnitOfWork uow,
    ILogger<RefreshExchangeRatesCommandHandler> logger)
    : IRequestHandler<RefreshExchangeRatesCommand, Result>
{
    public async Task<Result> Handle(RefreshExchangeRatesCommand request, CancellationToken ct)
    {
        logger.LogInformation("EVT=AUTO_JOB_START CAT=AUTO ACT=EXEC JOB=ExchangeRateRefresh NOTE=fetching_ecb");

        var rates = await provider.GetLatestRatesAsync(ct);

        if (rates.Count == 0)
        {
            logger.LogWarning("EVT=AUTO_JOB_END CAT=AUTO ACT=EXEC OUT=WARN JOB=ExchangeRateRefresh NOTE=no_rates_returned");
            return Result.Success();
        }

        // Get existing records so we can update rather than always inserting
        var existing = await repository.GetAllAsync(ct);
        var existingByCode = existing.ToDictionary(r => r.CurrencyCode, StringComparer.OrdinalIgnoreCase);

        var toUpsert = new List<ExchangeRate>(rates.Count);

        foreach (var dto in rates)
        {
            if (existingByCode.TryGetValue(dto.CurrencyCode, out var current))
            {
                current.UpdateRate(dto.Rate, dto.RateDate);
                toUpsert.Add(current);
            }
            else
            {
                toUpsert.Add(ExchangeRate.Create(
                    IdGenerator.New(IdPrefix.ExchangeRate),
                    dto.CurrencyCode,
                    dto.CurrencyName,
                    dto.Rate,
                    dto.RateDate
                ));
            }
        }

        await repository.UpsertAsync(toUpsert, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "EVT=AUTO_JOB_END CAT=AUTO ACT=EXEC OUT=OK JOB=ExchangeRateRefresh CNT={Count}",
            toUpsert.Count);

        return Result.Success();
    }
}
