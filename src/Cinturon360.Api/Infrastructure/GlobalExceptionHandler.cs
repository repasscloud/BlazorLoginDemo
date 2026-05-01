using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Infrastructure;

/// <summary>
/// Global exception handler that maps well-known application exceptions to RFC 7807 ProblemDetails responses.
/// Registered via app.UseExceptionHandler() + services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;().
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationEx:
            {
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new ValidationProblemDetails(errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title  = "One or more validation errors occurred.",
                        Type   = "https://tools.ietf.org/html/rfc7807"
                    },
                    cancellationToken);
                return true;
            }

            case UnauthorizedAccessException:
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title  = "Forbidden",
                        Detail = "You do not have permission to perform this action.",
                        Type   = "https://tools.ietf.org/html/rfc7807"
                    },
                    cancellationToken);
                return true;
            }

            default:
            {
                logger.LogError(exception, "Unhandled exception");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title  = "An unexpected error occurred.",
                        Type   = "https://tools.ietf.org/html/rfc7807"
                    },
                    cancellationToken);
                return true;
            }
        }
    }
}
