using Cinturon360.Contracts.Common.Errors;

namespace Cinturon360.Contracts.Common.Results;

/// <summary>Standard envelope for all API responses.</summary>
public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    ApiError? Error = null
);

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data) => new(true, data);
    public static ApiResponse<T> Fail<T>(ApiError error) => new(false, default, error);
    public static ApiResponse<object> Fail(ApiError error) => new(false, null, error);
}
