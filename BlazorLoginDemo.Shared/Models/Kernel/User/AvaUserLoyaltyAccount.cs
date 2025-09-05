using BlazorLoginDemo.Shared.Models.Kernel.Transactions;

namespace BlazorLoginDemo.Shared.Models.User
{
    public sealed class AvaUserLoyaltyAccount
    {
        public Guid Id { get; set; }

        // FK -> AvaUser (string PK)
        public string AvaUserId { get; set; } = default!;
        public AvaUser AvaUser { get; set; } = default!;

        // FK -> LoyaltyProgram (already present)
        public int LoyaltyProgramId { get; set; }
        public LoyaltyProgram Program { get; set; } = default!;

        public string MembershipNumber { get; set; } = default!;
        public string? Tier { get; set; }
        public DateTime? TierExpiryUtc { get; set; }
        public bool IsPreferred { get; set; }
        public bool IsAutoSelectEnabled { get; set; } = true;

        public string? FirstNameOnAccount { get; set; }
        public string? LastNameOnAccount { get; set; }
    }
}
