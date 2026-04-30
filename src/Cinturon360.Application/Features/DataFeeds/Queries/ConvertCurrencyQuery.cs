using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.DataFeeds.Queries;

/// <summary>
/// Converts an amount from one currency to another using the latest stored ECB rates.
/// Conversion is always routed through EUR as the base:
///   amount_in_eur = amount / from_rate
///   result = amount_in_eur * to_rate
/// To get the EUR value of an amount, set ToCurrencyCode = "EUR".
/// </summary>
public sealed record ConvertCurrencyQuery(
    decimal Amount,
    string FromCurrencyCode,
    string ToCurrencyCode
) : IRequest<Result<decimal>>;
