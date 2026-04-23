namespace Cinturon360.Web.ViewModels.Orgs;

public sealed class ListOrgsVm
{
    public string Rid { get; }
    public bool IsLoading { get; }
    public string? Error { get; }
    public List<OrgListItemVm> Orgs { get; }

    private ListOrgsVm(
        string rid,
        bool isLoading,
        string? error,
        List<OrgListItemVm> orgs)
    {
        Rid = rid;
        IsLoading = isLoading;
        Error = error;
        Orgs = orgs;
    }

    public static ListOrgsVm Loading(string rid)
        => new(rid, true, null, new());

    public static ListOrgsVm Ready(
        string rid,
        List<OrgListItemVm> orgs)
        => new(rid, false, null, orgs);

    public static ListOrgsVm Failed(
        string error,
        string rid)
        => new(rid, false, error, new());
}