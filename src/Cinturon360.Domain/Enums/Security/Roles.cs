namespace Cinturon360.Domain.Enums.Security;

public enum AppRole
{
    GlobalAdmin  = 1,
    Support      = 2,
    Finance      = 3,
    Integrations = 4
}

public enum OrgRole
{
    OrgAdmin   = 1,
    Approver   = 2,
    Booker     = 3,
    Traveller  = 4,
    ReadOnly   = 5
}
