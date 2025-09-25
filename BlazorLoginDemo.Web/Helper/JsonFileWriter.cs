using System.Text.Json;

namespace BlazorLoginDemo.Web.Helpers;

public static class JsonFileWriter
{
    private static readonly string BaseDirectory = Path.Combine(AppContext.BaseDirectory, "JsonOutput");

    public static void WriteToJsonFile<T>(T obj, string fileName)
    {
        Directory.CreateDirectory(BaseDirectory);

        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        string fullPath = Path.Combine(BaseDirectory, fileName);

        var options = new JsonSerializerOptions(JsonDefaults.WebEnums)
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(obj, options);

        File.WriteAllText(fullPath, json);
    }
}