using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;

namespace Cinturon360.Application.Features.Auth.Queries;

public sealed class ListPatsQueryHandler(IUserApiTokenRepository tokenRepo)
    : IRequestHandler<ListPatsQuery, Result<IReadOnlyList<PatSummary>>>
{
    public async Task<Result<IReadOnlyList<PatSummary>>> Handle(ListPatsQuery request, CancellationToken ct)
    {
        var tokens = await tokenRepo.GetActiveByUserIdAsync(request.UserId, ct);

        var summaries = tokens
            .Select(t => new PatSummary(
                TokenId: t.Id,
                Name: t.Name,
                TokenPrefix: t.TokenPrefix,
                CreatedAt: t.CreatedAt,
                ExpiresAt: t.ExpiresAt,
                LastUsedAt: t.LastUsedAt,
                IsRevoked: t.IsRevoked))
            .ToList();

        return Result.Success<IReadOnlyList<PatSummary>>(summaries);
    }
}
