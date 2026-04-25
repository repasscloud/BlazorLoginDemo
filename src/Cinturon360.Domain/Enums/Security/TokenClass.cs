namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// Classification of a token / session.
/// </summary>
public enum TokenClass
{
    InteractiveWebSession  = 1,
    MobileSession          = 2,
    ShortLivedApiLogin     = 3,
    UserPat                = 4,
    ServiceAccountToken    = 5
}
