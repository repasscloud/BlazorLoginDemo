namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// Controls how far a role assignment can reach down the org hierarchy.
/// </summary>
public enum ScopeMode
{
    /// <summary>Acts only within the user's home org.</summary>
    Self                 = 1,

    /// <summary>Acts within home org and all orgs below it in the hierarchy.</summary>
    SelfAndDescendants   = 2
}
