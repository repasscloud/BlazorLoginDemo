namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// High-level classification of a user principal.
/// </summary>
public enum UserCategory
{
    Public   = 1,
    Client   = 2,
    Tmc      = 3,
    Vendor   = 4,
    Platform = 5
}
