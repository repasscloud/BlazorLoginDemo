using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Models.ExternalLib.GitHub;
public class GitHubRepoOAuthToken
{
    [Key]
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
} 