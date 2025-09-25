namespace BlazorLoginDemo.Web.Services;

public sealed class EmailAttachment
{
    public string FileName { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = Array.Empty<byte>();
}
