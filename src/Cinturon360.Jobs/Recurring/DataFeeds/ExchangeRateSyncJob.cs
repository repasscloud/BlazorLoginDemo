using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Features.DataFeeds.Commands;

namespace Cinturon360.Jobs.Recurring.DataFeeds;

/// <summary>
/// Recurring background job that fetches ECB exchange rates every 5 minutes.
/// Dispatches <see cref="RefreshExchangeRatesCommand"/> via MediatR so the
/// Application layer handles all DB/provider coordination.
/// </summary>
public sealed class ExchangeRateSyncJob(
    IMediator mediator,
    ILogger<ExchangeRateSyncJob> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "EVT=AUTO_JOB_START CAT=AUTO ACT=START JOB=ExchangeRateSyncJob NOTE=interval_5min");

        // Run immediately on startup, then repeat on interval
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                var result = await mediator.Send(new RefreshExchangeRatesCommand(), stoppingToken);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "EVT=AUTO_JOB_END CAT=AUTO ACT=EXEC OUT=WARN JOB=ExchangeRateSyncJob NOTE={Error}",
                        result.Error.Description);
                }
            }
            catch (OperationCanceledException)
            {
                // Shutdown — exit cleanly
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "EVT=AUTO_JOB_ERR CAT=AUTO ACT=EXEC OUT=ERR JOB=ExchangeRateSyncJob NOTE=unhandled_exception");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));

        logger.LogInformation(
            "EVT=AUTO_JOB_END CAT=AUTO ACT=END JOB=ExchangeRateSyncJob NOTE=stopped");
    }
}
