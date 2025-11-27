using System.Diagnostics;
using Cinturon360.Shared.Models.Kernel.SysVar;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;

namespace Cinturon360.Api.Infrastructure.Middleware;

public sealed class ApiRequestLoggingMiddleware : IMiddleware
{
    private readonly ILoggerService _log;

    public ApiRequestLoggingMiddleware(ILoggerService log) => _log = log;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();

            var status = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = (context.Request.Path.Value ?? "/") + context.Request.QueryString.Value;

            var rid = RequestContext.GetCorrelationId(context);
            var tid = RequestContext.GetTraceId();

            var uid = RequestContext.GetUserId(context);
            var org = RequestContext.GetOrgId(context);

            var client = RequestContext.GetClient(context);
            var note = client is null ? null : $"client={client}";

            var evt = "HTTP_REQUEST";
            var cat = SysLogCatType.Api;
            var act = MapAction(method);

            if (status is >= 200 and <= 399)
            {
                await _log.InformationAsync(
                    evt, cat, act,
                    message: $"{method} {path}",
                    rid: rid, tid: tid, uid: uid, org: org,
                    durMs: (int)sw.ElapsedMilliseconds,
                    http: method, stat: status, path: path,
                    note: note,
                    overrideOutcome: SysLogOutcome.OK);
            }
            else if (status is >= 400 and <= 499)
            {
                await _log.WarningAsync(
                    evt, cat, act,
                    message: $"{method} {path}",
                    ex: null,
                    rid: rid, tid: tid, uid: uid, org: org,
                    durMs: (int)sw.ElapsedMilliseconds,
                    http: method, stat: status, path: path,
                    note: note,
                    overrideOutcome: SysLogOutcome.WARN);
            }
            else
            {
                // No exception object here (the exception middleware logs real exceptions).
                // This is just "request ended 5xx".
                await _log.LogAsync(
                    level: SysLogLevel.Error,
                    evt: evt,
                    cat: cat,
                    act: act,
                    outcome: SysLogOutcome.ERR,
                    message: $"{method} {path}",
                    ex: null,
                    rid: rid, tid: tid, uid: uid, org: org,
                    durMs: (int)sw.ElapsedMilliseconds,
                    http: method, stat: status, path: path,
                    note: note);
            }
        }
    }

    private static SysLogActionType MapAction(string method) => method.ToUpperInvariant() switch
    {
        "GET" or "HEAD" => SysLogActionType.Read,
        "POST" => SysLogActionType.Create,
        "PUT" or "PATCH" => SysLogActionType.Update,
        "DELETE" => SysLogActionType.Delete,
        _ => SysLogActionType.Read
    };
}
