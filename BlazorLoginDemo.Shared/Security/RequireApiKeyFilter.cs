using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BlazorLoginDemo.Shared.Security;

public sealed class RequireApiKeyFilter : IAsyncActionFilter
{
    private readonly InboundApiKeyOptions _opts;

    public RequireApiKeyFilter(IOptions<InboundApiKeyOptions> opts) => _opts = opts.Value;

    public Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var req = ctx.HttpContext.Request;

        if (!req.Headers.TryGetValue(_opts.HeaderName, out var provided) ||
            string.IsNullOrWhiteSpace(provided))
        {
            return SetUnauthorized(ctx, "Missing API key");
        }

        var providedBytes = Encoding.UTF8.GetBytes(provided.ToString().Trim());

        var ok = _opts.AllowedKeys.Any(k =>
        {
            var kb = Encoding.UTF8.GetBytes(k);
            return kb.Length == providedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(kb, providedBytes);
        });

        if (!ok)
            return SetUnauthorized(ctx, "Invalid API key");

        return next();
    }

    private static Task SetUnauthorized(ActionExecutingContext ctx, string reason)
    {
        ctx.HttpContext.Response.Headers["WWW-Authenticate"] = "ApiKey";
        ctx.Result = new UnauthorizedObjectResult(new { error = reason });
        return Task.CompletedTask;
    }
}
