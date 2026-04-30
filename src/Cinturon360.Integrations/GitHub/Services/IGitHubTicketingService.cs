using Cinturon360.Integrations.GitHub.Models;

namespace Cinturon360.Integrations.GitHub.Services;

/// <summary>
/// Abstraction for GitHub Issues operations used by the ticketing system.
/// </summary>
public interface IGitHubTicketingService
{
    /// <summary>Create a new GitHub issue. Returns the created issue or null on failure.</summary>
    Task<GitHubIssueResponse?> CreateIssueAsync(
        string title,
        string body,
        IReadOnlyList<string> labels,
        CancellationToken ct = default);

    /// <summary>Add a comment to an existing issue. Returns comment ID or null on failure.</summary>
    Task<GitHubCommentResponse?> AddCommentAsync(
        int issueNumber,
        string body,
        CancellationToken ct = default);

    /// <summary>Replace the full label set on an issue.</summary>
    Task<bool> SetLabelsAsync(
        int issueNumber,
        IReadOnlyList<string> labels,
        CancellationToken ct = default);

    /// <summary>Close an issue.</summary>
    Task<bool> CloseIssueAsync(int issueNumber, CancellationToken ct = default);
}
