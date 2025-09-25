using System.Text.Json;
using System.Text.Json.Serialization;
using Macross.Json.Extensions;

namespace BlazorLoginDemo.Web.Helpers;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions WebEnums = Create();

    private static JsonSerializerOptions Create()
    {
        var o = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        o.Converters.Add(new JsonStringEnumConverter());
        o.Converters.Add(new JsonStringEnumMemberConverter());
        return o;
    }
}