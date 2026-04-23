using System.Security.Cryptography;
using System.Text;

namespace Cinturon360.Common.IdGeneration;

/// <summary>
/// Generates prefixed, URL-safe, human-readable IDs.
/// Format: {prefix}_{randomPart}
/// </summary>
public static class IdGenerator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
    private const int DefaultLength = 16;

    public static string New(string prefix, int length = DefaultLength)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Prefix must not be empty.", nameof(prefix));

        var randomPart = GenerateNanoId(length);
        return $"{prefix}_{randomPart}";
    }

    public static string NewUserId()           => New(IdPrefix.User);
    public static string NewOrgId()            => New(IdPrefix.Organization);
    public static string NewRoleId()           => New(IdPrefix.Role);
    public static string NewBookingId()        => New(IdPrefix.Booking);
    public static string NewQuoteId()          => New(IdPrefix.Quote);
    public static string NewSessionId()        => New(IdPrefix.Session);
    public static string NewApiTokenId()       => New(IdPrefix.ApiToken);
    public static string NewServiceAccountId() => New(IdPrefix.ServiceAccount);
    public static string NewTicketId()         => New(IdPrefix.Ticket);
    public static string NewDocumentId()       => New(IdPrefix.Document);
    public static string NewJobId()            => New(IdPrefix.Job);
    public static string NewInvoiceId()        => New(IdPrefix.Invoice);
    public static string NewPaymentId()        => New(IdPrefix.Payment);
    public static string NewLicenseId()        => New(IdPrefix.License);
    public static string NewAuditEventId()     => New(IdPrefix.AuditEvent);

    private static string GenerateNanoId(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length * 2);
        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            var idx = b % Alphabet.Length;
            sb.Append(Alphabet[idx]);
            if (sb.Length == length) break;
        }
        return sb.ToString();
    }
}
