using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.DataFeeds.Commands;

/// <summary>
/// Deletes exchange-rate snapshots older than the retention window.
/// </summary>
public sealed record CleanupExchangeRateSnapshotsCommand(TimeSpan Retention) : IRequest<Result<int>>;
