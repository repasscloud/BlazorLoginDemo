using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Helpers;
using Cinturon360.Shared.Models.Kernel.Travel;

namespace Cinturon360.Shared.Models.User;
public sealed class AvaUserLoyaltyAccount
{
    [Key, MaxLength(20)]
    public string Id { get; set; } = IDGeneratorHelper.GenerateId(IdGenType.LoyaltyAccount);

    // FK -> AvaUser (string PK)
    public string AvaUserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;

    // FK -> LoyaltyProgram (already present)
    public int LoyaltyProgramId { get; set; }
    public LoyaltyProgram Program { get; set; } = default!;

    [MaxLength(80)] public string MembershipNumber { get; set; } = default!;
    public string? Tier { get; set; }
    public DateTime? TierExpiryUtc { get; set; }
    public bool IsPreferred { get; set; }
    public bool IsAutoSelectEnabled { get; set; } = true;

    public string? FirstNameOnAccount { get; set; }
    public string? LastNameOnAccount { get; set; }
}
