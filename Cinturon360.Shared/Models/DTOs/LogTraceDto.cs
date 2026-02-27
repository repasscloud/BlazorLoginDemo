namespace Cinturon360.Shared.Models.DTOs;

public sealed class LogTraceDto
{
    public string? RID { get; set; }   // request id
    public Guid? TID { get; set; }   // transaction id
    public string? ORG { get; set; }   // organization id
    public string? UID { get; init; }  // actor tracing (usr id)
}