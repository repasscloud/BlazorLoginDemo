namespace Cinturon360.Web.Helpers;

public static class RequestIdFactory
{
    public static string Create()
        => $"{Guid.NewGuid():N}";
}