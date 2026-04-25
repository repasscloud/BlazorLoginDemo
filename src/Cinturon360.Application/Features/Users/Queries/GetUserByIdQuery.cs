using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Users;

namespace Cinturon360.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(string UserId) : IRequest<Result<UserDetail>>;
