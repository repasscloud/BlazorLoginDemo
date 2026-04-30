using System.Net.Http.Json;
using System.Text.Json;
using Cinturon360.Contracts.Common.Results;

namespace Cinturon360.Web.Services.ApiClients;

/// <summary>
/// Base class for all typed API clients.
/// Provides shared JSON serialisation options and a consistent error mapping strategy.
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient Http;
    protected readonly ILogger Logger;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    protected ApiClientBase(HttpClient http, ILogger logger)
    {
        Http   = http;
        Logger = logger;
    }

    protected async Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.GetAsync(path, ct);
            return await MapResponseAsync<T>(response, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "GET {Path} failed", path);
            return ApiResult<T>.NetworkError("Unable to reach the server. Please check your connection.");
        }
    }

    protected async Task<ApiResult<T>> PostAsync<T>(string path, object body, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.PostAsJsonAsync(path, body, JsonOptions, ct);
            return await MapResponseAsync<T>(response, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "POST {Path} failed", path);
            return ApiResult<T>.NetworkError("Unable to reach the server. Please check your connection.");
        }
    }

    protected async Task<ApiResult<T>> PutAsync<T>(string path, object body, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.PutAsJsonAsync(path, body, JsonOptions, ct);
            return await MapResponseAsync<T>(response, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "PUT {Path} failed", path);
            return ApiResult<T>.NetworkError("Unable to reach the server. Please check your connection.");
        }
    }

    protected async Task<ApiResult<T>> PostMultipartAsync<T>(string path, MultipartFormDataContent form, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.PostAsync(path, form, ct);
            return await MapResponseAsync<T>(response, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "POST(multipart) {Path} failed", path);
            return ApiResult<T>.NetworkError("Unable to reach the server. Please check your connection.");
        }
    }

    protected async Task<ApiResult<bool>> DeleteAsync(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.DeleteAsync(path, ct);
            if (response.IsSuccessStatusCode)
                return ApiResult<bool>.Ok(true);

            var error = await ReadErrorAsync(response, ct);
            return ApiResult<bool>.Fail(error);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "DELETE {Path} failed", path);
            return ApiResult<bool>.NetworkError("Unable to reach the server. Please check your connection.");
        }
    }

    private static async Task<ApiResult<T>> MapResponseAsync<T>(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            if (string.IsNullOrWhiteSpace(body))
            {
                if (typeof(T) == typeof(bool))
                {
                    return ApiResult<T>.Ok((T)(object)true);
                }

                return ApiResult<T>.Fail("The server returned an empty response.");
            }

            try
            {
                var envelope = JsonSerializer.Deserialize<ApiResponse<T>>(body, JsonOptions);
                if (envelope is not null && (envelope.Data is not null || envelope.Error is not null || body.Contains("\"success\"", StringComparison.OrdinalIgnoreCase)))
                {
                    if (envelope.Success && envelope.Data is not null)
                    {
                        return ApiResult<T>.Ok(envelope.Data);
                    }

                    var envelopeError = envelope.Error?.Message ?? "The server returned an invalid response.";
                    return ApiResult<T>.Fail(envelopeError);
                }
            }
            catch
            {
                // Fall back to raw payload parsing for endpoints that are not wrapped.
            }

            var result = JsonSerializer.Deserialize<T>(body, JsonOptions);
            if (result is not null)
            {
                return ApiResult<T>.Ok(result);
            }

            return ApiResult<T>.Fail("The server returned an unreadable response.");
        }

        var error = await ReadErrorAsync(response, ct);
        return ApiResult<T>.Fail(error);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errEl) &&
                    (errEl.TryGetProperty("description", out var descEl) ||
                     errEl.TryGetProperty("message", out descEl)))
                {
                    return descEl.GetString() ?? response.ReasonPhrase ?? "Unknown error";
                }
            }
        }
        catch { /* fall through to status code */ }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized  => "You are not signed in or your session has expired.",
            System.Net.HttpStatusCode.Forbidden     => "You do not have permission to perform this action.",
            System.Net.HttpStatusCode.NotFound      => "The requested resource was not found.",
            System.Net.HttpStatusCode.Conflict      => "A conflict occurred. The resource may already exist.",
            System.Net.HttpStatusCode.UnprocessableEntity => "Validation failed. Please check your input.",
            System.Net.HttpStatusCode.TooManyRequests     => "Too many requests. Please try again shortly.",
            _ => $"An unexpected error occurred ({(int)response.StatusCode}).",
        };
    }
}

/// <summary>
/// Wrapper around an API response for use in Blazor components.
/// </summary>
public sealed class ApiResult<T>
{
    public bool IsSuccess { get; private init; }
    public T?   Value     { get; private init; }
    public string? Error  { get; private init; }
    public bool IsNetworkError { get; private init; }

    public static ApiResult<T> Ok(T value)
        => new() { IsSuccess = true, Value = value };

    public static ApiResult<T> Fail(string error)
        => new() { IsSuccess = false, Error = error };

    public static ApiResult<T> NetworkError(string message)
        => new() { IsSuccess = false, Error = message, IsNetworkError = true };
}
