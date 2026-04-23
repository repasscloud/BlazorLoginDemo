using static Cinturon360.Shared.Contracts.Policies.TravelPolicyUnifiedDto;

namespace Cinturon360.Web.ViewModels.Policies.Travel;

public static class ListOrgTravelPoliciesMapper
{
    public static ListOrgTravelPoliciesVm ToVm(
        ListTravelPolicyItemsAggregate aggregate,
        string rid)
    {
        var items =
            aggregate.TravelPolicyItems
                .Select(p => new TravelPolicyListItemVm
                {
                    Id = p.Id,
                    Name = p.Name,
                    EffectiveFromUtc = p.EffectiveFromUtc,
                    Status = p.Status
                })
                .ToList();

        return ListOrgTravelPoliciesVm.Ready(rid, items);
    }
}