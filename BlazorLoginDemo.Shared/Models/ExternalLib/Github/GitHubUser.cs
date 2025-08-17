using System.Text.Json.Serialization;

namespace BlazorLoginDemo.Shared.Models.ExternalLib.GitHub;
public class GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = "";
}
