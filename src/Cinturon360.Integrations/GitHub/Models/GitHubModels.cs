using System.Text.Json.Serialization;

namespace Cinturon360.Integrations.GitHub.Models;

// ── Requests ──────────────────────────────────────────────────────────────

public sealed record CreateIssueRequest(
    [property: JsonPropertyName("title")]  string Title,
    [property: JsonPropertyName("body")]   string Body,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels);

public sealed record UpdateIssueRequest(
    [property: JsonPropertyName("title")]  string? Title  = null,
    [property: JsonPropertyName("state")]  string? State  = null,   // "open" | "closed"
    [property: JsonPropertyName("labels")] IReadOnlyList<string>? Labels = null);

public sealed record CreateCommentRequest(
    [property: JsonPropertyName("body")] string Body);

// ── Responses ─────────────────────────────────────────────────────────────

public sealed record GitHubIssueResponse(
    [property: JsonPropertyName("number")] int    Number,
    [property: JsonPropertyName("html_url")] string HtmlUrl,
    [property: JsonPropertyName("title")]  string Title,
    [property: JsonPropertyName("state")]  string State,
    [property: JsonPropertyName("body")]   string? Body);

public sealed record GitHubCommentResponse(
    [property: JsonPropertyName("id")]   long   Id,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("html_url")] string HtmlUrl);
