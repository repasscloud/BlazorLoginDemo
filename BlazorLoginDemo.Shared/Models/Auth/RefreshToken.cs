using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorLoginDemo.Shared.Models.User;

namespace BlazorLoginDemo.Shared.Models.Auth;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public string Token { get; set; } = default!;          // opaque, random
    [Required] public DateTime ExpiresUtc { get; set; }
    [Required] public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public string? CreatedByIp { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }                      // for rotation
    public string? ReasonRevoked { get; set; }

    // FK to your user (Identity user or your own)
    [Required] public string UserId { get; set; } = default!;
    public AvaUser User { get; set; } = default!;                     // adjust type if different
}
