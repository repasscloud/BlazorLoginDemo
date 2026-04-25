namespace Cinturon360.Domain.Enums.Security;

/// <summary>
/// How the user account was provisioned.
/// </summary>
public enum ProvisioningSource
{
    Manual        = 1,
    JitOidc       = 2,
    JitSaml       = 3,
    Scim          = 4,
    CsvImport     = 5,
    PublicSignup  = 6
}
