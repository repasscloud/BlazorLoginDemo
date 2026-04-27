using Cinturon360.Contracts.Auth;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class IdentityApiClient : ApiClientBase
{
    public IdentityApiClient(HttpClient http, ILogger<IdentityApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => PostAsync<AuthResponse>("api/v1/auth/login", request, ct);

    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => PostAsync<AuthResponse>("api/v1/auth/register", request, ct);

    public Task<ApiResult<IReadOnlyList<PatSummary>>> ListPatsAsync(CancellationToken ct = default)
        => GetAsync<IReadOnlyList<PatSummary>>("api/v1/auth/pat", ct);

    public Task<ApiResult<CreatePatResponse>> CreatePatAsync(CreatePatRequest request, CancellationToken ct = default)
        => PostAsync<CreatePatResponse>("api/v1/auth/pat", request, ct);

    public Task<ApiResult<bool>> RevokePatAsync(string tokenId, CancellationToken ct = default)
        => DeleteAsync($"api/v1/auth/pat/{Uri.EscapeDataString(tokenId)}", ct);

    public Task<ApiResult<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
        => PostAsync<AuthResponse>("api/v1/auth/refresh", request, ct);

    public Task<ApiResult<bool>> LogoutAsync(string sessionId, CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/auth/logout?sessionId={Uri.EscapeDataString(sessionId)}", new { }, ct);

    public Task<ApiResult<MfaChallengeResponse>> VerifyMfaAsync(MfaChallengeRequest request, CancellationToken ct = default)
        => PostAsync<MfaChallengeResponse>("api/v1/auth/mfa/verify", request, ct);

    public Task<ApiResult<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => PostAsync<bool>("api/v1/auth/password/forgot", request, ct);

    public Task<ApiResult<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => PostAsync<bool>("api/v1/auth/password/reset", request, ct);
}
