using Cinturon360.Shared.Models.Policies;

namespace Cinturon360.Shared.Contracts.Policies;

// ===============================
// TRAVEL POLICY (UNIFIED) DTO
// ===============================
public class TravelPolicyUnifiedDto
{
    // convenience aggregates
    public sealed record TravelPolicyAggregate(
        IReadOnlyList<TravelPolicy> TravelPolicies,
        CommandMetadata Metadata
    );

    public sealed record CreateTravelPolicyAggregate(
        CreateTravelPolicyRequest CreateTravelPolicy,
        CommandMetadata Metadata
    );

    public sealed record UpdateTravelPolicyAggregate(
        UpdateTravelPolicyRequest UpdateTravelPolicy,
        CommandMetadata Metadata
    );

    public sealed record TravelPolicyNoResponseAggregate(
        string TravelPolicyId,
        CommandMetadata Metadata
    );

    public sealed record ListTravelPolicyItemsAggregate(
        IReadOnlyList<TravelPolicyListItem> TravelPolicyItems,
        CommandMetadata Metadata
    );
}
