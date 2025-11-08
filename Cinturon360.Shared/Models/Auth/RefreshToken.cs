using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.User;

namespace Cinturon360.Shared.Models.Auth;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public string Token { get; set; } = default!;
    [Required] public DateTime ExpiresUtc { get; set; }
    [Required] public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public string? CreatedByIp { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    // FK -> AvaUser (note: AvaUser.Id is string)
    [Required] public string AvaUserId { get; set; } = default!;
    public AvaUser AvaUser { get; set; } = default!;
}
