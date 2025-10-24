using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BlazorLoginDemo.Shared.Services.Helpers;

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string,string>> Cache = new();

    public static string ToDisplayName<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var map = Cache.GetOrAdd(type, _ =>
            type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(
                    f => f.Name,
                    f => f.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? f.Name));

        return map[value.ToString()];
    }
}