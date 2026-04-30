using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.DataFeeds.Queries;

public sealed class ConvertCurrencyQueryHandler(IExchangeRateRepository repository)
    : IRequestHandler<ConvertCurrencyQuery, Result<decimal>>
{
    private static readonly Error RateNotFound =
        new("FX.RateNotFound", "Exchange rate not available for one or more requested currencies.");

    private static readonly Error InvalidAmount =
        new("FX.InvalidAmount", "Amount must be greater than zero.");

    public async Task<Result<decimal>> Handle(ConvertCurrencyQuery request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return Result.Failure<decimal>(InvalidAmount);

        var from = request.FromCurrencyCode.ToUpperInvariant();
        var to   = request.ToCurrencyCode.ToUpperInvariant();

        if (from == to)
            return Result.Success(request.Amount);

        // EUR is the base — no lookup needed for EUR side
        decimal fromRate = 1m;
        decimal toRate   = 1m;

        if (from != "EUR")
        {
            var fromEntity = await repository.GetByCurrencyCodeAsync(from, ct);
            if (fromEntity is null) return Result.Failure<decimal>(RateNotFound);
            fromRate = fromEntity.Rate;
        }

        if (to != "EUR")
        {
            var toEntity = await repository.GetByCurrencyCodeAsync(to, ct);
            if (toEntity is null) return Result.Failure<decimal>(RateNotFound);
            toRate = toEntity.Rate;
        }

        // Convert via EUR: amount / fromRate * toRate
        var result = Math.Round(request.Amount / fromRate * toRate, 4, MidpointRounding.AwayFromZero);

        return Result.Success(result);
    }
}
