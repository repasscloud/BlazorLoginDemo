using Amazon.S3;
using Amazon.S3.Model;
using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cinturon360.Infrastructure.Storage;

public sealed class S3StorageSettings
{
    public string BucketName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;      // R2 endpoint
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// S3-compatible (Cloudflare R2) storage service.
/// </summary>
internal sealed class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly S3StorageSettings _settings;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3, IOptions<S3StorageSettings> settings, ILogger<S3StorageService> logger)
    {
        _s3 = s3;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        bool isPublic = false,
        CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
        };

        await _s3.PutObjectAsync(request, ct);
        _logger.LogInformation("Uploaded {Key} to bucket {Bucket}", key, _settings.BucketName);
        return key;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken ct = default)
    {
        var response = await _s3.GetObjectAsync(_settings.BucketName, key, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_settings.BucketName, key, ct);
        _logger.LogInformation("Deleted {Key} from bucket {Bucket}", key, _settings.BucketName);
    }

    public Task<string> GetPresignedUrlAsync(string key, int expiresInSeconds = 3600, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            Verb = HttpVerb.GET
        };
        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
}
