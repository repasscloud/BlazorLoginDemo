// BlazorLoginDemo.Shared/Services/Kernel/LoggerService.cs
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.SysVar;
using BlazorLoginDemo.Shared.Models.Static.SysVar;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.Extensions.Configuration;

namespace BlazorLoginDemo.Shared.Services.Kernel
{
    public sealed class LoggerService : ILoggerService
    {
        private readonly ApplicationDbContext _db;
        private readonly SysLogLevel _minLevel;

        public LoggerService(ApplicationDbContext db, IConfiguration cfg)
        {
            _db = db;
            _minLevel = MapLevel(cfg["Logging:LogLevel:Default"]);
        }

        public Task VerboseAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null)
            => LogCoreAsync(SysLogLevel.Verbose, overrideOutcome ?? SysLogOutcome.OK,
                            evt, cat, act, message, null, ent, entId, rid, tid, uid, org,
                            durMs, http, stat, path, note);

        public Task DebugAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null)
            => LogCoreAsync(SysLogLevel.Debug, overrideOutcome ?? SysLogOutcome.OK,
                            evt, cat, act, message, null, ent, entId, rid, tid, uid, org,
                            durMs, http, stat, path, note);

        public Task InformationAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null)
            => LogCoreAsync(SysLogLevel.Information, overrideOutcome ?? SysLogOutcome.OK,
                            evt, cat, act, message, null, ent, entId, rid, tid, uid, org,
                            durMs, http, stat, path, note);

        public Task WarningAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, Exception? ex = null,
            string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null)
            => LogCoreAsync(SysLogLevel.Warning, overrideOutcome ?? SysLogOutcome.WARN,
                            evt, cat, act, message, ex, ent, entId, rid, tid, uid, org,
                            durMs, http, stat, path, note);

        public Task ErrorAsync(string evt, SysLogCatType cat, SysLogActionType act,
            Exception ex, string? message = null,
            string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null)
            => LogCoreAsync(SysLogLevel.Error, overrideOutcome ?? SysLogOutcome.ERR,
                            evt, cat, act, message, ex, ent, entId, rid, tid, uid, org,
                            durMs, http, stat, path, note);

        public Task FatalAsync(string evt, SysLogCatType cat, SysLogActionType act,
            Exception ex, string? message = null,
            string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null)
            => LogCoreAsync(SysLogLevel.Fatal, overrideOutcome ?? SysLogOutcome.FAIL,
                            evt, cat, act, message, ex, ent, entId, rid, tid, uid, org,
                            durMs, http, stat, path, note);

        public Task LogAsync(SysLogLevel level, string evt, SysLogCatType cat, SysLogActionType act,
            SysLogOutcome outcome, string? message = null, Exception? ex = null,
            string? ent = null, string? entId = null, string? rid = null,
            string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null)
            => LogCoreAsync(level, outcome, evt, cat, act, message, ex, ent, entId, rid,
                            tid, uid, org, durMs, http, stat, path, note);

        // ---- Core ----
        private async Task LogCoreAsync(SysLogLevel level, SysLogOutcome outcome,
            string evt, SysLogCatType cat, SysLogActionType act,
            string? message, Exception? ex,
            string? ent, string? entId, string? rid,
            string? tid, string? uid, string? org,
            int? durMs, string? http, int? stat, string? path, string? note)
        {
            if (level < _minLevel) return;

            var entry = new AvaSystemLog
            {
                Timestamp = DateTimeOffset.UtcNow,
                Level = level,
                Evt = evt,
                Cat = cat,
                Act = act,
                Out = outcome,
                Ent = ent,
                EntId = entId,
                Rid = string.IsNullOrWhiteSpace(rid) ? Guid.NewGuid().ToString("N") : rid!,
                Tid = tid,
                Uid = uid,
                Org = org,
                DurMs = durMs,
                Http = http,
                Stat = stat,
                Path = path,
                Note = note,
                Message = ComposeMessage(message, ex)
            };

            _db.AvaSystemLogs.Add(entry);
            await _db.SaveChangesAsync();
        }

        private static string ComposeMessage(string? message, Exception? ex)
        {
            if (ex is null) return message ?? string.Empty;
            var prefix = string.IsNullOrWhiteSpace(message) ? "Exception" : message;
            return $"{prefix} | {ex.GetType().Name}: {ex.Message}";
        }

        private static SysLogLevel MapLevel(string? raw)
        {
            if (Enum.TryParse<SysLogLevel>(raw, true, out var lvl)) return lvl;

            return raw?.ToLowerInvariant() switch
            {
                "trace" => SysLogLevel.Verbose,
                "debug" => SysLogLevel.Debug,
                "information" => SysLogLevel.Information,
                "warn" or "warning" => SysLogLevel.Warning,
                "error" => SysLogLevel.Error,
                "critical" or "fatal" => SysLogLevel.Fatal,
                _ => SysLogLevel.Information
            };
        }
    }
}
