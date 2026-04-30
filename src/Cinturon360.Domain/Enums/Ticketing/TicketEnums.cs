namespace Cinturon360.Domain.Enums.Ticketing;

/// <summary>
/// Lifecycle status of a support ticket.
/// </summary>
public enum TicketStatus
{
    Open         = 1,
    InProgress   = 2,
    AwaitingUser = 3,  // support waiting on user reply
    Escalated    = 4,
    Resolved     = 5,
    Closed       = 6   // closed tickets cannot be re-opened via the platform
}

/// <summary>
/// Priority level. Escalation bumps priority automatically.
/// </summary>
public enum TicketPriority
{
    Low      = 1,
    Medium   = 2,
    High     = 3,
    Critical = 4
}

/// <summary>
/// Which support queue owns the ticket at any point in time.
/// Escalation moves it up the hierarchy; de-escalation moves it back.
/// </summary>
public enum TicketQueue
{
    Client  = 1,  // handled by TMC support for the client
    Tmc     = 2,  // escalated to TMC-level support
    Vendor  = 3   // escalated to Vendor (GPS) support
}

/// <summary>
/// Category of the issue, used for GitHub label routing.
/// </summary>
public enum TicketCategory
{
    General          = 1,
    BookingIssue     = 2,
    PaymentIssue     = 3,
    PolicyQuestion   = 4,
    ProfileIssue     = 5,
    SystemError      = 6,  // raised from an error page — contains technical context
    AccessDenied     = 7,
    TravelSearch     = 8,
    ApprovalIssue    = 9,
    Other            = 99
}
