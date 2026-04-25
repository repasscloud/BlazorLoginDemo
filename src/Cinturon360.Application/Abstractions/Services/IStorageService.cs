namespace Cinturon360.Application.Abstractions.Services;

/// <summary>
/// Abstraction for object storage (Cloudflare R2 / S3-compatible).
/// </summary>
public interface IStorageService
{
    Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        bool isPublic = false,
        CancellationToken ct = default);

    Task<Stream> DownloadAsync(string key, CancellationToken ct = default);

    Task DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>Generate a pre-signed URL valid for <paramref name="expiresInSeconds"/> seconds.</summary>
    Task<string> GetPresignedUrlAsync(string key, int expiresInSeconds = 3600, CancellationToken ct = default);
}
