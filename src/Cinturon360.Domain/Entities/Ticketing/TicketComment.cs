using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Ticketing;

/// <summary>
/// A comment or reply on a support ticket.
/// IsPrivate = true: only visible to users with a support role; mirrored as [PRIVATE NOTE] on GitHub.
/// AuthorDisplayName: resolved at creation time from the org SupportTeamName (for support authors)
///   or the user's first name (for end-user authors).
/// </summary>
public sealed class TicketComment : Entity
{
    public string   TicketId          { get; private set; } = string.Empty;
    public string   AuthorUserId      { get; private set; } = string.Empty;

    /// <summary>
    /// Display name shown on the comment — first name for users, support team name for support authors.
    /// Captured at write time so renames don't change history.
    /// </summary>
    public string   AuthorDisplayName { get; private set; } = string.Empty;

    /// <summary>True if this is a private note — hidden from the ticket requester.</summary>
    public bool     IsPrivate         { get; private set; }

    public string   Body              { get; private set; } = string.Empty;

    /// <summary>GitHub comment ID returned after syncing to GitHub Issues. Null until synced.</summary>
    public long?    GitHubCommentId   { get; private set; }

    private TicketComment() { }

    public static TicketComment Create(
        string id,
        string ticketId,
        string authorUserId,
        string authorDisplayName,
        string body,
        bool isPrivate = false)
    {
        return new TicketComment
        {
            Id                = id,
            CreatedAt         = DateTimeOffset.UtcNow,
            UpdatedAt         = DateTimeOffset.UtcNow,
            TicketId          = ticketId,
            AuthorUserId      = authorUserId,
            AuthorDisplayName = authorDisplayName,
            Body              = body.Trim(),
            IsPrivate         = isPrivate
        };
    }

    public void SetGitHubCommentId(long commentId)
    {
        GitHubCommentId = commentId;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }
}
