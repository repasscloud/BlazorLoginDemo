using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Contracts;

public sealed class CommandMetadata
{
    /// <summary>
    /// Request / correlation ID.
    /// Must remain stable across UI → API → downstream calls.
    /// </summary>
    [Required, MaxLength(64)]
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Optional transaction scope ID.
    /// One per logical business operation.
    /// </summary>
    public Guid? TransactionId { get; init; }

    /// <summary>
    /// Authenticated user ID (ApplicationUser.Id).
    /// </summary>
    [MaxLength(128)]
    public string? UserId { get; init; }

    /// <summary>
    /// Owning organisation (Unified org ID).
    /// </summary>
    [MaxLength(64)]
    public string? OrganizationId { get; init; }
}
