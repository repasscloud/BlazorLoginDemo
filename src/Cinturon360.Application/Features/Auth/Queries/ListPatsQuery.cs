using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;

namespace Cinturon360.Application.Features.Auth.Queries;

/// <summary>Lists all active PATs for a user.</summary>
public sealed record ListPatsQuery(string UserId) : IRequest<Result<IReadOnlyList<PatSummary>>>;
