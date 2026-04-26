namespace Cinturon360.Contracts.Policies;

/// <summary>List wrapper for travel policies.</summary>
public sealed record PolicyListResponse(IReadOnlyList<TravelPolicyResponse> Items);
