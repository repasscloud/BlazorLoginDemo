namespace Cinturon360.Contracts.Users;

public sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string UserCategory,
    string? HomeOrgId = null,
    string? Password = null
);

public sealed record UserSummary(
    string UserId,
    string Email,
    string FullName,
    string UserCategory,
    string? HomeOrgId,
    bool IsActive,
    bool IsLocked,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt
);

public sealed record UserDetail(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string UserCategory,
    string? PlatformRole,
    string? HomeOrgId,
    bool IsActive,
    bool IsLocked,
    bool IsSuspended,
    bool IsEmailVerified,
    string LanguageCode,
    string TimeZone,
    string CurrencyCode,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt
);
