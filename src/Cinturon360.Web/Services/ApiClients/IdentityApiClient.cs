using Cinturon360.Contracts.Auth;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class IdentityApiClient : ApiClientBase
{
    public IdentityApiClient(HttpClient http, ILogger<IdentityApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => PostAsync<LoginResponse>("api/v1/auth/login", request, ct);

    public Task<ApiResult<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
        => PostAsync<AuthResponse>("api/v1/auth/refresh", request, ct);

    public Task<ApiResult<bool>> LogoutAsync(CancellationToken ct = default)
        => PostAsync<bool>("api/v1/auth/logout", new { }, ct);

    public Task<ApiResult<MfaChallengeResponse>> VerifyMfaAsync(MfaChallengeRequest request, CancellationToken ct = default)
        => PostAsync<MfaChallengeResponse>("api/v1/auth/mfa/verify", request, ct);

    public Task<ApiResult<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => PostAsync<bool>("api/v1/auth/password/forgot", request, ct);

    public Task<ApiResult<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => PostAsync<bool>("api/v1/auth/password/reset", request, ct);
}
