namespace Cinturon360.Shared.Contracts.Policies;

// ===============================
// TRAVEL POLICY (UNIFIED) DTO
// ===============================
public class TravelPolicyUnifiedDto
{
    // convenience aggregates
    public sealed record TravelPolicyAggregate(
        IReadOnlyList<Models.Policies.TravelPolicy> TravelPolicies,
        CommandMetadata Metadata
    );

    public sealed record CreateTravelPolicyAggregate(
        CreateTravelPolicyRequest CreateTravelPolicy,
        CommandMetadata Metadata
    );
}
