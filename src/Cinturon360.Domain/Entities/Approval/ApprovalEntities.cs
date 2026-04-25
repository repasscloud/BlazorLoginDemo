using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Approval;

namespace Cinturon360.Domain.Entities.Approval;

/// <summary>
/// An approval request raised for a booking or other subject.
/// Has N levels; each level requires a specific approver.
/// </summary>
public sealed class ApprovalRequest : Entity
{
    public string OrgId { get; private set; } = string.Empty;
    public ApprovalSubjectType SubjectType { get; private set; }
    public string SubjectId { get; private set; } = string.Empty;  // BookingId, QuoteId, etc.
    public string RequestedByUserId { get; private set; } = string.Empty;
    public ApprovalStatus Status { get; private set; } = ApprovalStatus.Pending;

    public int TotalLevels { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    public string? Notes { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    private ApprovalRequest() { }

    public static ApprovalRequest Create(
        string id,
        string orgId,
        ApprovalSubjectType subjectType,
        string subjectId,
        string requestedByUserId,
        int totalLevels,
        string? notes = null)
        => new()
        {
            Id = id,
            OrgId = orgId,
            SubjectType = subjectType,
            SubjectId = subjectId,
            RequestedByUserId = requestedByUserId,
            TotalLevels = totalLevels,
            CurrentLevel = 1,
            Status = ApprovalStatus.Pending,
            Notes = notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void AdvanceLevel()
    {
        if (CurrentLevel < TotalLevels)
        {
            CurrentLevel++;
        }
        else
        {
            Status = ApprovalStatus.Approved;
            ResolvedAt = DateTimeOffset.UtcNow;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string? reason = null)
    {
        Status = ApprovalStatus.Rejected;
        if (reason is not null) Notes = reason;
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = ApprovalStatus.Cancelled;
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// A single approval decision at one level of an ApprovalRequest.
/// </summary>
public sealed class ApprovalDecision : Entity
{
    public string ApprovalRequestId { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public string ApproverUserId { get; private set; } = string.Empty;
    public ApprovalStatus Decision { get; private set; }
    public string? Comments { get; private set; }

    private ApprovalDecision() { }

    public static ApprovalDecision Create(
        string id,
        string approvalRequestId,
        int level,
        string approverUserId,
        ApprovalStatus decision,
        string? comments = null)
        => new()
        {
            Id = id,
            ApprovalRequestId = approvalRequestId,
            Level = level,
            ApproverUserId = approverUserId,
            Decision = decision,
            Comments = comments,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
