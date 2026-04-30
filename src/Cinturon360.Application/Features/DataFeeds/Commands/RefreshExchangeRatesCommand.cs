using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.DataFeeds.Commands;

/// <summary>
/// Triggers a full refresh of ECB exchange rates from the provider.
/// Intended to be dispatched by the recurring data-feed job every 5 minutes.
/// </summary>
public sealed record RefreshExchangeRatesCommand : IRequest<Result>;
