using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Models.ExternalLib.GitHub;

public class GitHubComment
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = "";

    [JsonPropertyName("user")]
    public GitHubUser User { get; set; } = new();

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";
}
