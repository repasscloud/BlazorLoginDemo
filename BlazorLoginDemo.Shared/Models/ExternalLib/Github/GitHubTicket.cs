using System.Text.Json.Serialization;

namespace BlazorLoginDemo.Shared.Models.ExternalLib.GitHub;

public class GitHubTicket
{
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string State { get; set; } = "open";
    public List<GitHubLabel> Labels { get; set; } = new();

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("comments")]
    public int Comments { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";

    [JsonPropertyName("user")]
    public GitHubUser User { get; set; } = new();

    [JsonPropertyName("assignee")]
    public GitHubUser? Assignee { get; set; }
}
