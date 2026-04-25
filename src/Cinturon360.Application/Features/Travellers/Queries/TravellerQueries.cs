using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Travellers.Queries;

// ── Get profile ───────────────────────────────────────────────────────────
public sealed record GetTravellerProfileQuery(string UserId) : IRequest<Result<TravellerProfile?>>;

public sealed class GetTravellerProfileHandler : IRequestHandler<GetTravellerProfileQuery, Result<TravellerProfile?>>
{
    private readonly ITravellerProfileRepository _repo;
    public GetTravellerProfileHandler(ITravellerProfileRepository repo) => _repo = repo;

    public async Task<Result<TravellerProfile?>> Handle(GetTravellerProfileQuery request, CancellationToken ct)
    {
        var profile = await _repo.GetByUserIdAsync(request.UserId, ct);
        return Result.Success(profile);
    }
}

// ── Get loyalty programs ──────────────────────────────────────────────────
public sealed record GetLoyaltyProgramsQuery(string UserId) : IRequest<Result<IReadOnlyList<TravellerLoyaltyProgram>>>;

public sealed class GetLoyaltyProgramsHandler : IRequestHandler<GetLoyaltyProgramsQuery, Result<IReadOnlyList<TravellerLoyaltyProgram>>>
{
    private readonly ITravellerProfileRepository _repo;
    public GetLoyaltyProgramsHandler(ITravellerProfileRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<TravellerLoyaltyProgram>>> Handle(GetLoyaltyProgramsQuery request, CancellationToken ct)
    {
        var programs = await _repo.GetLoyaltyProgramsAsync(request.UserId, ct);
        return Result.Success(programs);
    }
}

// ── Get emergency contacts ────────────────────────────────────────────────
public sealed record GetEmergencyContactsQuery(string UserId) : IRequest<Result<IReadOnlyList<UserEmergencyContact>>>;

public sealed class GetEmergencyContactsHandler : IRequestHandler<GetEmergencyContactsQuery, Result<IReadOnlyList<UserEmergencyContact>>>
{
    private readonly ITravellerProfileRepository _repo;
    public GetEmergencyContactsHandler(ITravellerProfileRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<UserEmergencyContact>>> Handle(GetEmergencyContactsQuery request, CancellationToken ct)
    {
        var contacts = await _repo.GetEmergencyContactsAsync(request.UserId, ct);
        return Result.Success(contacts);
    }
}

// ── Get preferences ───────────────────────────────────────────────────────
public sealed record GetPreferencesQuery(string UserId) : IRequest<Result<UserPreferences?>>;

public sealed class GetPreferencesHandler : IRequestHandler<GetPreferencesQuery, Result<UserPreferences?>>
{
    private readonly ITravellerProfileRepository _repo;
    public GetPreferencesHandler(ITravellerProfileRepository repo) => _repo = repo;

    public async Task<Result<UserPreferences?>> Handle(GetPreferencesQuery request, CancellationToken ct)
    {
        var prefs = await _repo.GetPreferencesAsync(request.UserId, ct);
        return Result.Success(prefs);
    }
}
