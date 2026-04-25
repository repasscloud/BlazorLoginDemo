namespace Cinturon360.Domain.Enums.Approval;

public enum ApprovalStatus
{
    Pending   = 1,
    Approved  = 2,
    Rejected  = 3,
    Cancelled = 4,
    Escalated = 5
}

public enum ApprovalSubjectType
{
    Booking = 1,
    Quote   = 2,
    Expense = 3
}
