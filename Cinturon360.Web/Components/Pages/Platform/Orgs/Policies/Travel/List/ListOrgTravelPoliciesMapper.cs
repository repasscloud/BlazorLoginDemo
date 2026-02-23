namespace Cinturon360.Web.ViewModels.Policies.Travel;

public static class ListOrgTravelPoliciesMapper
{

    public static ListOrgTravelPoliciesVm ToVm(
        ListTravelPoliciesResponse response,
        string rid)
    {

        var items =
            response.Policies
                .Select(p => new TravelPolicyListItemVm
                {
                    Id = p.Id,
                    Name = p.Name,
                    EffectiveFromUtc = p.EffectiveFromUtc,
                    Status = p.Status
                })
                .ToList();

        return
            ListOrgTravelPoliciesVm.Ready(rid, items);

    }

}