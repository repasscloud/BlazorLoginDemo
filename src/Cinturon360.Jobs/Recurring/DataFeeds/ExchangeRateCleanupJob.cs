using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Features.DataFeeds.Commands;

namespace Cinturon360.Jobs.Recurring.DataFeeds;

/// <summary>
/// Runs once per UTC day and deletes exchange-rate snapshots older than 24 hours.
/// </summary>
public sealed class ExchangeRateCleanupJob(
    IMediator mediator,
    ILogger<ExchangeRateCleanupJob> logger)
    : BackgroundService
{
    private static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "EVT=AUTO_JOB_START CAT=AUTO ACT=START JOB=ExchangeRateCleanupJob NOTE=daily_utc_midnight");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = GetDelayUntilNextUtcMidnight();
                await Task.Delay(delay, stoppingToken);

                var result = await mediator.Send(
                    new CleanupExchangeRateSnapshotsCommand(Retention),
                    stoppingToken);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "EVT=AUTO_JOB_END CAT=AUTO ACT=EXEC OUT=WARN JOB=ExchangeRateCleanupJob NOTE={Error}",
                        result.Error.Description);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "EVT=AUTO_JOB_ERR CAT=AUTO ACT=EXEC OUT=ERR JOB=ExchangeRateCleanupJob NOTE=unhandled_exception");
            }
        }

        logger.LogInformation(
            "EVT=AUTO_JOB_END CAT=AUTO ACT=END JOB=ExchangeRateCleanupJob NOTE=stopped");
    }

    private static TimeSpan GetDelayUntilNextUtcMidnight()
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var nextUtcMidnight = new DateTimeOffset(
            nowUtc.Year,
            nowUtc.Month,
            nowUtc.Day,
            0,
            0,
            0,
            TimeSpan.Zero).AddDays(1);

        return nextUtcMidnight - nowUtc;
    }
}
