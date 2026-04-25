namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// Platform-level roles for sudo / ops accounts that have no home org.
/// Standard tenant users do NOT use this — they use OrgRole via UserRoleAssignment.
/// </summary>
public enum PlatformRole
{
    Sudo          = 1,
    PlatformOps   = 2,
    PlatformAudit = 3
}

/// <summary>
/// Organisation-scoped roles assigned via UserRoleAssignment.
/// </summary>
public enum OrgRole
{
    OrgAdmin   = 1,
    Approver   = 2,
    Booker     = 3,
    Traveller  = 4,
    ReadOnly   = 5
}
