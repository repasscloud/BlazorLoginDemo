namespace Cinturon360.Web.ViewModels.Policies.Travel;

public sealed class TravelPolicyListItemVm
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required DateTime EffectiveFromUtc { get; init; }
    public required string Status { get; init; }
}