namespace Cinturon360.Shared.Models.Policies;

public sealed record TravelPolicyListItem(
    string Id,
    string Name,
    DateTime EffectiveFromUtc,
    string Status
);