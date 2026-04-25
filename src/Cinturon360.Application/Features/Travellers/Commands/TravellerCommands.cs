using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Travellers.Commands;

// ── Get or create profile ─────────────────────────────────────────────────
public sealed record EnsureTravellerProfileCommand(string UserId) : IRequest<Result<TravellerProfile?>>;

public sealed class EnsureTravellerProfileHandler : IRequestHandler<EnsureTravellerProfileCommand, Result<TravellerProfile?>>
{
    private readonly ITravellerProfileRepository _repo;
    private readonly IUnitOfWork _uow;

    public EnsureTravellerProfileHandler(ITravellerProfileRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<TravellerProfile?>> Handle(EnsureTravellerProfileCommand request, CancellationToken ct)
    {
        var existing = await _repo.GetByUserIdAsync(request.UserId, ct);
        if (existing is not null) return Result.Success<TravellerProfile?>(existing);

        var profile = TravellerProfile.Create(IdGenerator.New(IdPrefix.TravellerProfile), request.UserId);
        await _repo.AddAsync(profile, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success<TravellerProfile?>(profile);
    }
}

// ── Update passport ───────────────────────────────────────────────────────
public sealed record UpdatePassportCommand(
    string UserId,
    string? PassportNumber,
    string? PassportCountry,
    DateOnly? PassportExpiry,
    string? Nationality) : IRequest<Result>;

public sealed class UpdatePassportHandler : IRequestHandler<UpdatePassportCommand, Result>
{
    private readonly ITravellerProfileRepository _repo;
    private readonly IUnitOfWork _uow;

    public UpdatePassportHandler(ITravellerProfileRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdatePassportCommand request, CancellationToken ct)
    {
        var profile = await _repo.GetByUserIdAsync(request.UserId, ct);
        if (profile is null) return Result.Failure(TravellerErrors.ProfileNotFound);

        profile.UpdatePassport(request.PassportNumber, request.PassportCountry, request.PassportExpiry, request.Nationality);
        _repo.Update(profile);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Update preferences ────────────────────────────────────────────────────
public sealed record UpdateTravelPreferencesCommand(
    string UserId,
    string? AirportCode,
    string? AirlineCode,
    string? HotelChain,
    string? CarRental,
    bool NotifyByEmail,
    bool NotifyBySms,
    bool NotifyBookingConfirmation,
    bool NotifyApprovalRequired,
    bool NotifyApprovalDecision,
    bool NotifyTripReminders) : IRequest<Result>;

public sealed class UpdateTravelPreferencesHandler : IRequestHandler<UpdateTravelPreferencesCommand, Result>
{
    private readonly ITravellerProfileRepository _repo;
    private readonly IUnitOfWork _uow;

    public UpdateTravelPreferencesHandler(ITravellerProfileRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateTravelPreferencesCommand request, CancellationToken ct)
    {
        var prefs = await _repo.GetPreferencesAsync(request.UserId, ct);
        if (prefs is null)
        {
            prefs = UserPreferences.Create(IdGenerator.New(IdPrefix.Preferences), request.UserId);
            await _repo.AddPreferencesAsync(prefs, ct);
        }

        prefs.UpdateTravelPreferences(request.AirportCode, request.AirlineCode, request.HotelChain, request.CarRental);
        prefs.UpdateNotifications(
            request.NotifyByEmail, request.NotifyBySms,
            request.NotifyBookingConfirmation, request.NotifyApprovalRequired,
            request.NotifyApprovalDecision, request.NotifyTripReminders);

        _repo.UpdatePreferences(prefs);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Add loyalty program ───────────────────────────────────────────────────
public sealed record AddLoyaltyProgramCommand(
    string UserId,
    string ProgramCode,
    string ProgramName,
    string MembershipNumber,
    string? TierName,
    DateOnly? ExpiryDate) : IRequest<Result<string>>;

public sealed class AddLoyaltyProgramHandler : IRequestHandler<AddLoyaltyProgramCommand, Result<string>>
{
    private readonly ITravellerProfileRepository _repo;
    private readonly IUnitOfWork _uow;

    public AddLoyaltyProgramHandler(ITravellerProfileRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(AddLoyaltyProgramCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.LoyaltyProgram);
        var program = TravellerLoyaltyProgram.Create(
            id, request.UserId, request.ProgramCode, request.ProgramName,
            request.MembershipNumber, request.TierName, request.ExpiryDate);

        await _repo.AddLoyaltyProgramAsync(program, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Add emergency contact ─────────────────────────────────────────────────
public sealed record AddEmergencyContactCommand(
    string UserId,
    string Name,
    string Relationship,
    string Phone,
    string? Email,
    bool IsPrimary) : IRequest<Result<string>>;

public sealed class AddEmergencyContactHandler : IRequestHandler<AddEmergencyContactCommand, Result<string>>
{
    private readonly ITravellerProfileRepository _repo;
    private readonly IUnitOfWork _uow;

    public AddEmergencyContactHandler(ITravellerProfileRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(AddEmergencyContactCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.EmergencyContact);
        var contact = UserEmergencyContact.Create(
            id, request.UserId, request.Name, request.Relationship,
            request.Phone, request.Email, request.IsPrimary);

        await _repo.AddEmergencyContactAsync(contact, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}
