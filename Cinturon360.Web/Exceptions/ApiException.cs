using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Web.Exceptions;

public sealed class ApiException : Exception
{
    public ProblemDetails? Problem { get; }
    public HttpStatusCode StatusCode { get; }
    public string RequestId { get; }

    public ApiException(
        ProblemDetails? problem,
        HttpStatusCode statusCode,
        string requestId)
        : base(BuildMessage(problem, statusCode, requestId))
    {
        Problem = problem;
        StatusCode = statusCode;
        RequestId = requestId;
    }

    private static string BuildMessage(
        ProblemDetails? problem,
        HttpStatusCode statusCode,
        string requestId)
    {
        if (problem is not null)
        {
            var errorCode = problem.Extensions.TryGetValue("errorCode", out var ec)
                ? ec?.ToString()
                : "unknown";

            return $"API error {statusCode} ({errorCode}) — {problem.Title} (RID: {requestId})";
        }

        return $"API error {statusCode} (RID: {requestId})";
    }
}
