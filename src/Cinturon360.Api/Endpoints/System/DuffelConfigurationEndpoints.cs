using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Features.Integrations.Duffel.Commands;
using Cinturon360.Application.Features.Integrations.Duffel.Queries;
using Cinturon360.Common.Constants;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.System;
using Cinturon360.Domain.Enums.System;
using AppClaimTypes = Cinturon360.Common.Constants.ClaimTypes;

namespace Cinturon360.Api.Endpoints.System;

public static class DuffelConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapDuffelConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/system/duffel-configs")
            .WithTags("System")
            .RequireAuthorization();

        group.MapGet("/{orgId}", GetConfiguration)
            .WithName("GetDuffelOrgConfiguration")
            .Produces<ApiResponse<DuffelOrgConfigurationResponse>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404);

        group.MapPut("/{orgId}", UpsertConfiguration)
            .WithName("UpsertDuffelOrgConfiguration")
            .Produces<ApiResponse<DuffelOrgConfigurationResponse>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404);

        group.MapGet("/{orgId}/effective", GetEffectiveForOrg)
            .WithName("GetEffectiveDuffelConfigsForOrg")
            .Produces<ApiResponse<EffectiveDuffelConfigResponse>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404);

        return app;
    }

    private static async Task<IResult> GetConfiguration(
        HttpContext httpContext,
        string orgId,
        ISender mediator,
        IOrganisationRepository organisationRepository)
    {
        var auth = await AuthorizeForOrgConfigAsync(httpContext.User, orgId, organisationRepository);
        if (!auth.Allowed)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var result = await mediator.Send(new GetDuffelOrgConfigurationQuery(orgId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("duffel.config_not_found", "Duffel configuration not found.")))
            : Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> UpsertConfiguration(
        HttpContext httpContext,
        string orgId,
        [FromBody] UpsertDuffelOrgConfigurationRequest request,
        ISender mediator,
        IOrganisationRepository organisationRepository)
    {
        var auth = await AuthorizeForOrgConfigAsync(httpContext.User, orgId, organisationRepository, requireWrite: true);
        if (!auth.Allowed)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var result = await mediator.Send(new UpsertDuffelOrgConfigurationCommand(
            OrgId: orgId,
            IsEnabled: request.IsEnabled,
            UseSandbox: request.UseSandbox,
            ApiBaseUrl: request.ApiBaseUrl,
            ApiToken: request.ApiToken,
            AccessScope: request.AccessScope,
            EnabledCapabilities: request.EnabledCapabilities ?? [],
            EnabledSearchFunctions: request.EnabledSearchFunctions ?? [],
            CorporateCodes: request.CorporateCodes ?? [],
            TourCodes: request.TourCodes ?? [],
            Notes: request.Notes,
            UpdatedByUserId: auth.UserId));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> GetEffectiveForOrg(
        HttpContext httpContext,
        string orgId,
        ISender mediator,
        IOrganisationRepository organisationRepository)
    {
        var auth = await AuthorizeForOrgConfigAsync(httpContext.User, orgId, organisationRepository);
        if (!auth.Allowed)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var result = await mediator.Send(new GetEffectiveDuffelConfigurationsForOrgQuery(orgId));
        if (result.IsFailure)
            return Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<(bool Allowed, string? UserId)> AuthorizeForOrgConfigAsync(
        ClaimsPrincipal user,
        string targetOrgId,
        IOrganisationRepository organisationRepository,
        bool requireWrite = false)
    {
        var callerUserId = user.FindFirstValue(AppClaimTypes.UserId);
        var callerOrgId = user.FindFirstValue(AppClaimTypes.OrgId);
        var appRole = user.FindFirstValue(AppClaimTypes.AppRole)
                   ?? user.FindFirstValue("c360:role")
                   ?? user.FindFirstValue(global::System.Security.Claims.ClaimTypes.Role);
        var orgRole = user.FindFirstValue(AppClaimTypes.OrgRole);

        var targetOrg = await organisationRepository.GetByIdAsync(targetOrgId);
        if (targetOrg is null || targetOrg.OrgType != OrgType.Tmc)
            return (false, callerUserId);

        // Vendor/global-admin/integrations can manage any TMC config.
        if (string.Equals(appRole, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(appRole, Roles.Integrations, StringComparison.OrdinalIgnoreCase))
            return (true, callerUserId);

        // Client orgs cannot manage Duffel provider configs.
        if (callerOrgId is null)
            return (false, callerUserId);

        var callerOrg = await organisationRepository.GetByIdAsync(callerOrgId);
        if (callerOrg is null || callerOrg.OrgType == OrgType.Client)
            return (false, callerUserId);

        // TMC can manage their own config only.
        if (!string.Equals(callerOrgId, targetOrgId, StringComparison.OrdinalIgnoreCase))
            return (false, callerUserId);

        if (!requireWrite)
            return (true, callerUserId);

        var canWrite = string.Equals(orgRole, Roles.OrgAdmin, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(appRole, Roles.Support, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(appRole, Roles.Integrations, StringComparison.OrdinalIgnoreCase);

        return (canWrite, callerUserId);
    }
}
