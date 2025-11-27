using System.Net.Mime;
using System.Text.Json;
using Cinturon360.Shared.Models.Kernel.SysVar;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Infrastructure.Middleware;

public sealed class ProblemDetailsExceptionMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILoggerService _log;

    public ProblemDetailsExceptionMiddleware(ILoggerService log) => _log = log;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected / request aborted. Usually don’t try to write a response.
            context.Response.StatusCode = 499; // common in proxies; optional
        }
        catch (Exception ex)
        {
            var (status, code, title) = Map(ex);

            var rid = RequestContext.GetCorrelationId(context);
            var tid = RequestContext.GetTraceId();
            var uid = RequestContext.GetUserId(context);
            var org = RequestContext.GetOrgId(context);

            await _log.ErrorAsync(
                evt: "UNHANDLED_EXCEPTION",
                cat: SysLogCatType.Api,
                act: SysLogActionType.Read,
                ex: ex,
                message: title,
                rid: rid, tid: tid, uid: uid, org: org,
                http: context.Request.Method,
                stat: status,
                path: context.Request.Path.Value ?? "/",
                overrideOutcome: SysLogOutcome.FAIL);

            if (context.Response.HasStarted) throw;

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var pd = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            };

            pd.Extensions["errorCode"] = code;
            pd.Extensions["correlationId"] = rid;
            pd.Extensions["traceId"] = tid;

            await context.Response.WriteAsync(JsonSerializer.Serialize(pd, JsonOptions));
        }
    }

    private static (int status, string code, string title) Map(Exception ex) => ex switch
    {
        ArgumentException => (StatusCodes.Status400BadRequest, "validation_failed", "Validation failed"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "not_found", "Resource not found"),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "forbidden", "Forbidden"),
        _ => (StatusCodes.Status500InternalServerError, "internal_error", "Unexpected server error")
    };
}
