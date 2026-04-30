using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Cinturon360.Integrations.GitHub.Models;

namespace Cinturon360.Integrations.GitHub.Services;

/// <summary>
/// GitHub Issues client that calls the GitHub REST API v3.
/// Uses the named HttpClient "GitHubTicketing" configured in DI.
///
/// When the PAT is not configured the client operates in dry-run mode:
/// create/comment calls log what would have been sent and return null,
/// so ticket data is still persisted locally without hard-failing.
/// </summary>
public sealed class GitHubIssuesClient : IGitHubTicketingService
{
    private readonly HttpClient _http;
    private readonly ILogger<GitHubIssuesClient> _logger;
    private readonly bool _configured;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower
    };

    public GitHubIssuesClient(
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubIssuesClient> logger,
        bool configured)
    {
        _http       = httpClientFactory.CreateClient("GitHubTicketing");
        _logger     = logger;
        _configured = configured;
    }

    public async Task<GitHubIssueResponse?> CreateIssueAsync(
        string title,
        string body,
        IReadOnlyList<string> labels,
        CancellationToken ct = default)
    {
        if (!_configured)
        {
            _logger.LogInformation(
                "EVT=GitHubTicketing CAT=Ticketing ACT=CreateIssue OUT=DryRun NOTE=PAT not configured; title={Title}",
                title);
            return null;
        }

        var request = new CreateIssueRequest(title, body, labels);
        var response = await _http.PostAsJsonAsync("issues", request, JsonOpts, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "EVT=GitHubTicketing CAT=Ticketing ACT=CreateIssue OUT=Fail STATUS={Status} BODY={Body}",
                (int)response.StatusCode, err);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GitHubIssueResponse>(JsonOpts, ct);
    }

    public async Task<GitHubCommentResponse?> AddCommentAsync(
        int issueNumber,
        string body,
        CancellationToken ct = default)
    {
        if (!_configured)
        {
            _logger.LogInformation(
                "EVT=GitHubTicketing CAT=Ticketing ACT=AddComment OUT=DryRun NOTE=PAT not configured; issue={Issue}",
                issueNumber);
            return null;
        }

        var request  = new CreateCommentRequest(body);
        var response = await _http.PostAsJsonAsync($"issues/{issueNumber}/comments", request, JsonOpts, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "EVT=GitHubTicketing CAT=Ticketing ACT=AddComment OUT=Fail ISSUE={Issue} STATUS={Status} BODY={Body}",
                issueNumber, (int)response.StatusCode, err);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GitHubCommentResponse>(JsonOpts, ct);
    }

    public async Task<bool> SetLabelsAsync(
        int issueNumber,
        IReadOnlyList<string> labels,
        CancellationToken ct = default)
    {
        if (!_configured) return true;  // no-op in dry-run

        var request  = new UpdateIssueRequest(Labels: labels);
        var response = await _http.PatchAsJsonAsync($"issues/{issueNumber}", request, JsonOpts, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "EVT=GitHubTicketing CAT=Ticketing ACT=SetLabels OUT=Fail ISSUE={Issue} STATUS={Status}",
                issueNumber, (int)response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<bool> CloseIssueAsync(int issueNumber, CancellationToken ct = default)
    {
        if (!_configured) return true;  // no-op in dry-run

        var request  = new UpdateIssueRequest(State: "closed");
        var response = await _http.PatchAsJsonAsync($"issues/{issueNumber}", request, JsonOpts, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "EVT=GitHubTicketing CAT=Ticketing ACT=CloseIssue OUT=Fail ISSUE={Issue} STATUS={Status}",
                issueNumber, (int)response.StatusCode);
            return false;
        }

        return true;
    }
}
