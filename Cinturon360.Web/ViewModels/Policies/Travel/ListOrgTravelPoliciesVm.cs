namespace Cinturon360.Web.ViewModels.Policies.Travel;

public sealed class ListOrgTravelPoliciesVm
{
    public string Rid { get; }
    public bool IsLoading { get; }
    public string? Error { get; }
    
    public List<TravelPolicyListItemVm> Policies { get; }

    private ListOrgTravelPoliciesVm(
        string rid,
        bool isLoading,
        string? error,
        List<TravelPolicyListItemVm> policies)
    {
        Rid = rid;
        IsLoading = isLoading;
        Error = error;
        Policies = policies;
    }

    public static ListOrgTravelPoliciesVm Loading(string rid)
        => new(rid, true, null, new());

    public static ListOrgTravelPoliciesVm Ready(
        string rid,
        List<TravelPolicyListItemVm> policies)
        => new(rid, false, null, policies);

    public static ListOrgTravelPoliciesVm Failed(
        string error,
        string rid)
        => new(rid, false, error, new());
}