namespace Cinturon360.Shared.Models.Kernel.SysVar;

public sealed class ErrorCodeUnified
{
    public long Id { get; set; }  // surrogate PK
    public string ErrorCode { get; set; } = null!;  // unique string, e.g. "AUTH001"
    public string Title { get; set; } = null!;  // short headline
    public string Message { get; set; } = null!;  // detailed explanation
    public string? Resolution { get; set; }  // optional "how to fix" guidance
    public string? ContactSupportLink { get; set; }  // optional link for support
    public bool IsClientFacing { get; set; }  // useful for filtering
    public bool IsInternalFacing { get; set; }  // useful for filtering

    // Always set by the database, never updated
    public DateTime CreatedOnUtc { get; private set; }
}
