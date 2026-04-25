using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Travellers.Commands;
using Cinturon360.Application.Features.Travellers.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Travellers;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Api.Endpoints.Travellers;

public static class TravellerEndpoints
{
    public static IEndpointRouteBuilder MapTravellerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/travellers").WithTags("Travellers").RequireAuthorization();

        group.MapGet("/{userId}/profile", GetProfile)
            .WithName("GetTravellerProfile")
            .Produces<ApiResponse<TravellerProfileResponse>>(200);

        group.MapPost("/{userId}/profile/ensure", EnsureProfile)
            .WithName("EnsureTravellerProfile")
            .Produces<ApiResponse<TravellerProfileResponse>>(200);

        group.MapPut("/{userId}/profile/passport", UpdatePassport)
            .WithName("UpdatePassport")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(404);

        group.MapGet("/{userId}/loyalty-programs", GetLoyaltyPrograms)
            .WithName("GetLoyaltyPrograms")
            .Produces<ApiResponse<IEnumerable<LoyaltyProgramResponse>>>(200);

        group.MapPost("/{userId}/loyalty-programs", AddLoyaltyProgram)
            .WithName("AddLoyaltyProgram")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapGet("/{userId}/emergency-contacts", GetEmergencyContacts)
            .WithName("GetEmergencyContacts")
            .Produces<ApiResponse<IEnumerable<EmergencyContactResponse>>>(200);

        group.MapPost("/{userId}/emergency-contacts", AddEmergencyContact)
            .WithName("AddEmergencyContact")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapGet("/{userId}/preferences", GetPreferences)
            .WithName("GetTravellerPreferences")
            .Produces<ApiResponse<UserPreferencesResponse>>(200);

        group.MapPut("/{userId}/preferences", UpdatePreferences)
            .WithName("UpdateTravellerPreferences")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> GetProfile(string userId, ISender mediator)
    {
        var result = await mediator.Send(new GetTravellerProfileQuery(userId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(MapProfile(result.Value)))
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> EnsureProfile(string userId, ISender mediator)
    {
        var result = await mediator.Send(new EnsureTravellerProfileCommand(userId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(MapProfile(result.Value)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> UpdatePassport(
        string userId,
        [FromBody] UpdatePassportRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new UpdatePassportCommand(
            userId,
            request.PassportNumber,
            request.PassportCountry,
            request.PassportExpiry,
            request.Nationality));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> GetLoyaltyPrograms(string userId, ISender mediator)
    {
        var result = await mediator.Send(new GetLoyaltyProgramsQuery(userId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value.Select(MapLoyaltyProgram)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> AddLoyaltyProgram(
        string userId,
        [FromBody] AddLoyaltyProgramRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new AddLoyaltyProgramCommand(
            userId,
            request.ProgramCode,
            request.ProgramName,
            request.MembershipNumber,
            request.TierName,
            request.ExpiryDate));

        return result.IsSuccess
            ? Results.Created($"/api/v1/travellers/{userId}/loyalty-programs", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> GetEmergencyContacts(string userId, ISender mediator)
    {
        var result = await mediator.Send(new GetEmergencyContactsQuery(userId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value.Select(MapEmergencyContact)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> AddEmergencyContact(
        string userId,
        [FromBody] AddEmergencyContactRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new AddEmergencyContactCommand(
            userId,
            request.Name,
            request.Relationship,
            request.Phone,
            request.Email,
            request.IsPrimary));

        return result.IsSuccess
            ? Results.Created($"/api/v1/travellers/{userId}/emergency-contacts", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> GetPreferences(string userId, ISender mediator)
    {
        var result = await mediator.Send(new GetPreferencesQuery(userId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(MapPreferences(result.Value)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> UpdatePreferences(
        string userId,
        [FromBody] UpdatePreferencesRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new UpdateTravelPreferencesCommand(
            userId,
            request.PreferredAirportCode,
            request.PreferredAirlineCode,
            request.PreferredHotelChain,
            request.PreferredCarRentalCompany,
            request.NotifyByEmail,
            request.NotifyBySms,
            request.NotifyBookingConfirmation,
            request.NotifyApprovalRequired,
            request.NotifyApprovalDecision,
            request.NotifyTripReminders));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    // ── Mappers ──────────────────────────────────────────────────────────
    private static TravellerProfileResponse? MapProfile(TravellerProfile? p) =>
        p is null ? null : new TravellerProfileResponse(
            p.UserId,
            p.PassportNumber,
            p.PassportCountry,
            p.PassportExpiry,
            p.Nationality,
            p.DateOfBirth,
            p.Gender,
            p.TsaPreCheckNumber,
            p.GlobalEntryNumber,
            p.RedressNumber,
            p.PreferredSeatType,
            p.PreferredMealType,
            p.VipLevel);

    private static LoyaltyProgramResponse MapLoyaltyProgram(TravellerLoyaltyProgram p) =>
        new(p.Id, p.ProgramCode, p.ProgramName, p.MembershipNumber, p.TierName, p.ExpiryDate);

    private static EmergencyContactResponse MapEmergencyContact(UserEmergencyContact c) =>
        new(c.Id, c.Name, c.Relationship, c.Phone, c.Email, c.IsPrimary);

    private static UserPreferencesResponse? MapPreferences(UserPreferences? p) =>
        p is null ? null : new UserPreferencesResponse(
            p.PreferredAirportCode,
            p.PreferredAirlineCode,
            p.PreferredHotelChain,
            p.PreferredCarRentalCompany,
            p.NotifyByEmail,
            p.NotifyBySms,
            p.NotifyBookingConfirmation,
            p.NotifyApprovalRequired,
            p.NotifyApprovalDecision,
            p.NotifyTripReminders);
}
