using Cinturon360.Shared.Models.Kernel.SysVar;
using Cinturon360.Shared.Models.Static.System.SysVar;

namespace Cinturon360.Shared.Services.Interfaces.Kernel;
public interface ILoggerService
{
    // Level-explicit helpers
    Task VerboseAsync(SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
        string? message = null, string? ent = null, string? entId = null,
        string? rid = null, Guid? tid = null, string? uid = null, string? org = null,
        int? durMs = null, string? http = null, int? stat = null, string? path = null,
        string? note = null, SysLogOutcome? overrideOutcome = null);

    Task DebugAsync(SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
        string? message = null, string? ent = null, string? entId = null,
        string? rid = null, Guid? tid = null, string? uid = null, string? org = null,
        int? durMs = null, string? http = null, int? stat = null, string? path = null,
        string? note = null, SysLogOutcome? overrideOutcome = null);

    Task InformationAsync(SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
        string? message = null, string? ent = null, string? entId = null,
        string? rid = null, Guid? tid = null, string? uid = null, string? org = null,
        int? durMs = null, string? http = null, int? stat = null, string? path = null,
        string? note = null, SysLogOutcome? overrideOutcome = null);

    Task WarningAsync(SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
        string? message = null, Exception? ex = null,
        string? ent = null, string? entId = null,
        string? rid = null, Guid? tid = null, string? uid = null, string? org = null,
        int? durMs = null, string? http = null, int? stat = null, string? path = null,
        string? note = null, SysLogOutcome? overrideOutcome = null);

    Task ErrorAsync(SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
        Exception ex, string? message = null,
        string? ent = null, string? entId = null,
        string? rid = null, Guid? tid = null, string? uid = null, string? org = null,
        int? durMs = null, string? http = null, int? stat = null, string? path = null,
        string? note = null, SysLogOutcome? overrideOutcome = null);

    Task FatalAsync(SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
        Exception ex, string? message = null,
        string? ent = null, string? entId = null,
        string? rid = null, Guid? tid = null, string? uid = null, string? org = null,
        int? durMs = null, string? http = null, int? stat = null, string? path = null,
        string? note = null, SysLogOutcome? overrideOutcome = null);

    // Lowest-level escape hatch if you need to set Level and Outcome explicitly
    Task LogAsync(SysLogLevel level, SysLogEvtType evt, SysLogCatType cat, SysLogActionType act,
                    SysLogOutcome outcome, string? message = null, Exception? ex = null,
                    string? ent = null, string? entId = null, string? rid = null,
                    Guid? tid = null, string? uid = null, string? org = null,
                    int? durMs = null, string? http = null, int? stat = null, string? path = null,
                    string? note = null);
}
