namespace Cinturon360.Shared.Models.DTOs;

public sealed class LogTraceDto
{
    public Guid Tid { get; set; }    // transaction id
    public string? Uid { get; set; } // user id
    public string? Org { get; set; } // organization id
}