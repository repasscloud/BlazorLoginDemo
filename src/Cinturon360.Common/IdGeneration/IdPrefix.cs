namespace Cinturon360.Common.IdGeneration;

/// <summary>
/// Domain entity type prefixes for human-readable IDs.
/// Format: {prefix}_{NanoId}  e.g. usr_abc123xyz
/// </summary>
public static class IdPrefix
{
    public const string User           = "usr";
    public const string Organization   = "org";
    public const string Role           = "rol";
    public const string RoleAssignment = "rla";
    public const string ServiceAccount = "svc";
    public const string ApiToken       = "tok";
    public const string Session        = "ses";
    public const string Booking        = "bkg";
    public const string Quote          = "qte";
    public const string TravellerProfile = "tvl";
    public const string Policy         = "pol";
    public const string Approval       = "apr";
    public const string Ticket         = "tkt";
    public const string Document       = "doc";
    public const string Job            = "job";
    public const string Invoice        = "inv";
    public const string Payment        = "pay";
    public const string License        = "lic";
    public const string AuditEvent     = "aud";
}
