// BlazorLoginDemo.Shared/Services/Interfaces/Kernel/ILoggerService.cs
using System;
using System.Threading.Tasks;
using BlazorLoginDemo.Shared.Models.Kernel.SysVar;
using BlazorLoginDemo.Shared.Models.Static.SysVar;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Kernel
{
    public interface ILoggerService
    {
        // Level-explicit helpers
        Task VerboseAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null);

        Task DebugAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null);

        Task InformationAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null);

        Task WarningAsync(string evt, SysLogCatType cat, SysLogActionType act,
            string? message = null, Exception? ex = null,
            string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null);

        Task ErrorAsync(string evt, SysLogCatType cat, SysLogActionType act,
            Exception ex, string? message = null,
            string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
            int? durMs = null, string? http = null, int? stat = null, string? path = null,
            string? note = null, SysLogOutcome? overrideOutcome = null);

        Task FatalAsync(string evt, SysLogCatType cat, SysLogActionType act,
            Exception ex, string? message = null,
            string? ent = null, string? entId = null,
            string? rid = null, string? tid = null, string? uid = null, string? org = null,
                               int? durMs = null, string? http = null, int? stat = null, string? path = null,
                               string? note = null, SysLogOutcome? overrideOutcome = null);

        // Lowest-level escape hatch if you need to set Level and Outcome explicitly
        Task LogAsync(SysLogLevel level, string evt, SysLogCatType cat, SysLogActionType act,
                      SysLogOutcome outcome, string? message = null, Exception? ex = null,
                      string? ent = null, string? entId = null, string? rid = null,
                      string? tid = null, string? uid = null, string? org = null,
                      int? durMs = null, string? http = null, int? stat = null, string? path = null,
                      string? note = null);
    }
}
