// using Cinturon360.Shared.Data;
// using Cinturon360.Shared.Models.Kernel.SysVar;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace Cinturon360.Api.Controllers.Admin;

// [ApiController]
// [Route("v1/cron")]
// public sealed class CronController : ControllerBase
// {
//     private const string AmadeusTestHost = "test.api.amadeus.com";
//     private const int Port = 443;
//     private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

//     private readonly ApplicationDbContext _db;

//     public CronController(ApplicationDbContext db)
//     {
//         _db = db;
//     }

//     // ------ Expire ------
//     [HttpGet("expire/hourly")]
//     [ProducesResponseType(typeof(ExpireJobsResultDto), StatusCodes.Status200OK)]
//     public async Task<IActionResult> ExpireHourlyJobsAsync(CancellationToken ct)
//     {
//         var now = DateTime.UtcNow;
//         var cutoff = now.AddHours(-1);

//         var affected = await _db.QueuedJobs
//             .Where(j => HourlyJobTypesToExpire.Contains(j.JobType))
//             .Where(j => HourlyJobStatusesToExpire.Contains(j.Status))
//             .Where(j => j.CreatedUtc < cutoff)
//             .ExecuteUpdateAsync(setters => setters
//                 .SetProperty(j => j.Status, JobStatus.Expired)
//                 .SetProperty(j => j.AttemptCount, -1)
//                 .SetProperty(j => j.CompletedUtc, now),
//                 ct);

//         return Ok(new ExpireJobsResultDto
//         {
//             Expired = affected,
//             CutoffUtc = cutoff,
//             NowUtc = now
//         });
//     }

//     // ------ Amadeus ------
//     [HttpGet("extapi-check/amadeus-test")]
//     [ProducesResponseType(typeof(ProbeResultDto), StatusCodes.Status200OK)]
//     public async Task<IActionResult> CheckAmadeusTestAsync(CancellationToken ct)
//     {
//         var result = await CheckAsync(AmadeusTestHost);
//         return Ok(result);
//     }

//     // ------ DTOs ------
//     public sealed class ExpireJobsResultDto
//     {
//         public int Expired { get; init; }
//         public DateTime CutoffUtc { get; init; }
//         public DateTime NowUtc { get; init; }
//     }

//     public sealed class ProbeResultDto
//     {
//         public bool Success { get; init; }
//         public int? StatusCode { get; init; }
//         public long TotalMs { get; init; }
//         public string Message { get; init; } = "";
//         public string? Detail { get; init; }

//         public override string ToString()
//             => Success
//                 ? $"OK {StatusCode} ({TotalMs}ms)"
//                 : $"FAIL {Message} ({TotalMs}ms) {Detail}";
//     }

//     // ------ Static Data ------
//     private static readonly string[] HourlyJobTypesToExpire =
//     [
//         "FlightSearch"
//     ];

//     private static readonly JobStatus[] HourlyJobStatusesToExpire =
//     {
//         JobStatus.Unknown,
//         JobStatus.Pending,
//         JobStatus.Dequeued,
//         JobStatus.Leased,
//         JobStatus.Retrieved,
//         JobStatus.Heartbeating,
//         JobStatus.Processing,
//         JobStatus.Validating,
//         JobStatus.Enriching,
//         JobStatus.Executing,
//         JobStatus.Finalizing,
//         JobStatus.WaitingExternal,
//         JobStatus.WaitingRateLimit,
//         JobStatus.WaitingRetryBackoff,
//         JobStatus.WaitingManual,
//         JobStatus.Failed,
//         JobStatus.FailedValidation,
//         JobStatus.FailedExternal,
//         JobStatus.FailedTimeout,
//         JobStatus.FailedConflict,
//         JobStatus.FailedSecurity,
//         JobStatus.CancelRequested,
//         JobStatus.Cancelling,
//         JobStatus.Cancelled,
//         JobStatus.Superseded,
//         JobStatus.Reserved90,
//         JobStatus.Reserved91,
//         JobStatus.Reserved92,
//         JobStatus.Reserved93,
//         JobStatus.Reserved94,
//         JobStatus.Reserved95,
//         JobStatus.Reserved96,
//         JobStatus.Reserved97,
//         JobStatus.Reserved98,
//         JobStatus.Reserved99
//     };

//         private static ProbeResultDto Fail(
//         string reason,
//         System.Diagnostics.Stopwatch sw,
//         object? detail = null)
//     {
//         sw.Stop();
//         return new ProbeResultDto
//         {
//             Success = false,
//             TotalMs = sw.ElapsedMilliseconds,
//             Message = reason,
//             Detail = detail?.ToString()
//         };
//     }
//     public static async Task<ProbeResultDto> CheckAsync(string Host)
//     {
//         var sw = System.Diagnostics.Stopwatch.StartNew();

//         try
//         {
//             // 1. DNS
//             var dnsStart = sw.Elapsed;
//             var addresses = await System.Net.Dns.GetHostAddressesAsync(Host);
//             if (addresses.Length == 0)
//                 return Fail("DNS_RESOLUTION_FAILED", sw, dnsStart);

//             // 2. TCP connect (fast fail)
//             var tcpStart = sw.Elapsed;
//             using (var tcp = new System.Net.Sockets.TcpClient())
//             {
//                 using var cts = new CancellationTokenSource(Timeout);
//                 await tcp.ConnectAsync(Host, Port, cts.Token);
//             }

//             // 3 + 4. TLS + HTTP
//             var httpStart = sw.Elapsed;
//             using var handler = new SocketsHttpHandler
//             {
//                 ConnectTimeout = Timeout,
//                 PooledConnectionLifetime = TimeSpan.Zero
//             };

//             using var client = new HttpClient(handler)
//             {
//                 Timeout = Timeout
//             };

//             using var resp = await client.GetAsync(
//                 $"https://{Host}",
//                 HttpCompletionOption.ResponseHeadersRead
//             );

//             sw.Stop();

//             return new ProbeResultDto
//             {
//                 Success = true,
//                 StatusCode = (int)resp.StatusCode,
//                 TotalMs = sw.ElapsedMilliseconds,
//                 Message = "OK"
//             };
//         }
//         catch (OperationCanceledException)
//         {
//             return Fail("TIMEOUT", sw);
//         }
//         catch (HttpRequestException ex)
//         {
//             return Fail("HTTP_ERROR", sw, ex.Message);
//         }
//         catch (System.Net.Sockets.SocketException ex)
//         {
//             return Fail("SOCKET_ERROR", sw, ex.Message);
//         }
//         catch (Exception ex)
//         {
//             return Fail("UNEXPECTED_ERROR", sw, ex.ToString());
//         }
//     }
// }