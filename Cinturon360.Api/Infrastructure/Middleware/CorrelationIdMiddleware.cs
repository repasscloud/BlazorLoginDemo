using Microsoft.AspNetCore.Http;

namespace Cinturon360.Api.Infrastructure.Middleware;

public sealed class CorrelationIdMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Prefer incoming correlation id; otherwise generate one
        var incoming = context.Request.Headers[RequestContext.CorrelationHeader].ToString();
        var correlationId = string.IsNullOrWhiteSpace(incoming)
            ? Guid.NewGuid().ToString("N")
            : incoming;

        context.Items[RequestContext.ItemCorrelationId] = correlationId;
        context.Items[RequestContext.ItemTraceId] = RequestContext.GetTraceId();

        // Ensure headers are present even if the pipeline throws later
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[RequestContext.CorrelationHeader] = correlationId;
            context.Response.Headers[RequestContext.TraceHeader] = RequestContext.GetTraceId();
            context.Response.Headers["Cache-Control"] = "no-store";
            return Task.CompletedTask;
        });

        await next(context);
    }
}
