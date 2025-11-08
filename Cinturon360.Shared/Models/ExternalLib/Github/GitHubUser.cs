using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Models.ExternalLib.GitHub;
public class GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = "";
}
