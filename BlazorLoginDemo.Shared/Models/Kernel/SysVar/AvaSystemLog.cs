using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Models.Kernel.SysVar;

public class AvaSystemLog
{
    [Key]
    public long Id { get; set; }
    public string Level { get; set; } = string.Empty;  // TRACE, DEBUG, INFO, etc.
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
