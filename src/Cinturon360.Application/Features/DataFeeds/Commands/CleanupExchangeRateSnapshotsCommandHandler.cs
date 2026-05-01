using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.DataFeeds.Commands;

public sealed class CleanupExchangeRateSnapshotsCommandHandler(
    IExchangeRateRepository repository,
    ILogger<CleanupExchangeRateSnapshotsCommandHandler> logger)
    : IRequestHandler<CleanupExchangeRateSnapshotsCommand, Result<int>>
{
    private static readonly Error InvalidRetention =
        new("FX.InvalidRetention", "Retention must be greater than zero.");

    public async Task<Result<int>> Handle(CleanupExchangeRateSnapshotsCommand request, CancellationToken ct)
    {
        if (request.Retention <= TimeSpan.Zero)
            return Result.Failure<int>(InvalidRetention);

        var cutoff = DateTimeOffset.UtcNow.Subtract(request.Retention);
        var deleted = await repository.DeleteOlderThanAsync(cutoff, ct);

        logger.LogInformation(
            "EVT=AUTO_JOB_END CAT=AUTO ACT=EXEC OUT=OK JOB=ExchangeRateCleanup CNT={Deleted} NOTE=cutoff_{CutoffUtc}",
            deleted,
            cutoff);

        return Result.Success(deleted);
    }
}
