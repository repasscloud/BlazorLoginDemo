using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Ticketing;

/// <summary>
/// A file attached to a support ticket.
/// IsPrivate = true: only visible to support staff; hidden from the requester.
/// File content is stored as bytes in the database (PostgreSQL bytea).
/// Maximum enforced at the application layer (10 MB).
/// </summary>
public sealed class TicketAttachment : Entity
{
    public string TicketId          { get; private set; } = string.Empty;
    public string UploadedByUserId  { get; private set; } = string.Empty;

    /// <summary>Original filename, e.g. "screenshot.png".</summary>
    public string FileName          { get; private set; } = string.Empty;

    /// <summary>MIME type, e.g. "image/png".</summary>
    public string ContentType       { get; private set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long   FileSize          { get; private set; }

    /// <summary>Raw file bytes.</summary>
    public byte[] Content           { get; private set; } = [];

    /// <summary>When true, only support staff can see this attachment.</summary>
    public bool   IsPrivate         { get; private set; }

    private TicketAttachment() { }

    public static TicketAttachment Create(
        string id,
        string ticketId,
        string uploadedByUserId,
        string fileName,
        string contentType,
        byte[] content,
        bool   isPrivate = false)
    {
        return new TicketAttachment
        {
            Id               = id,
            TicketId         = ticketId,
            UploadedByUserId = uploadedByUserId,
            FileName         = fileName,
            ContentType      = contentType,
            FileSize         = content.Length,
            Content          = content,
            IsPrivate        = isPrivate,
            CreatedAt        = DateTimeOffset.UtcNow,
            UpdatedAt        = DateTimeOffset.UtcNow
        };
    }
}
