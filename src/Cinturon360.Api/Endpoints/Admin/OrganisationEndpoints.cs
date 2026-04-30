using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Organizations.Commands;
using Cinturon360.Application.Features.Organizations.Queries;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Contracts.Organizations;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Api.Endpoints.Admin;

public static class OrganisationEndpoints
{
    public static IEndpointRouteBuilder MapOrganisationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/organisations").WithTags("Organisations").RequireAuthorization();

        group.MapGet("/", ListOrgs)
            .WithName("ListOrganisations")
            .Produces<ApiResponse<IReadOnlyList<OrganisationSummary>>>(200);

        group.MapGet("/{orgId}", GetOrg)
            .WithName("GetOrganisation")
            .Produces<ApiResponse<OrganisationDetail>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapGet("/{orgId}/hierarchy", GetHierarchy)
            .WithName("GetOrganisationHierarchy")
            .Produces<ApiResponse<OrganisationHierarchyNode>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/", CreateOrg)
            .WithName("CreateOrganisation")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPut("/{orgId}", UpdateOrg)
            .WithName("UpdateOrganisation")
            .Produces(204)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/{orgId}/deactivate", DeactivateOrg)
            .WithName("DeactivateOrganisation")
            .Produces(204)
            .Produces<ApiResponse<object>>(404);

        group.MapPost("/{orgId}/reactivate", ReactivateOrg)
            .WithName("ReactivateOrganisation")
            .Produces(204)
            .Produces<ApiResponse<object>>(404);

        return app;
    }

    private static async Task<IResult> ListOrgs(
        ISender mediator,
        [FromQuery] string? parentOrgId = null,
        [FromQuery] string? orgType = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await mediator.Send(new ListOrganisationsQuery(parentOrgId, orgType, activeOnly, page, pageSize));
        return Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> GetOrg(string orgId, ISender mediator)
    {
        var result = await mediator.Send(new GetOrganisationByIdQuery(orgId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> GetHierarchy(string orgId, ISender mediator)
    {
        var result = await mediator.Send(new GetOrganisationHierarchyQuery(orgId));
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> CreateOrg([FromBody] CreateOrganisationRequest req, ISender mediator)
    {
        if (!Enum.TryParse<OrgType>(req.OrgType, ignoreCase: true, out var orgType))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("invalid_org_type", $"Unknown org type: {req.OrgType}")));

        var result = await mediator.Send(new CreateOrganisationCommand(
            Name: req.Name,
            Slug: req.Slug,
            OrgType: orgType,
            ParentOrgId: req.ParentOrgId,
            PrimaryEmail: req.PrimaryEmail,
            LanguageCode: req.LanguageCode,
            TimeZone: req.TimeZone,
            CurrencyCode: req.CurrencyCode));

        return result.IsSuccess
            ? Results.Created($"/api/v1/organisations/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> UpdateOrg(string orgId, [FromBody] UpdateOrganisationRequest req, ISender mediator)
    {
        var result = await mediator.Send(new UpdateOrganisationCommand(
            OrgId: orgId,
            Name: req.Name,
            PrimaryEmail: req.PrimaryEmail,
            PrimaryPhone: req.PrimaryPhone,
            Website: req.Website,
            SupportTicketEmailTemplateCode: req.SupportTicketEmailTemplateCode,
            LanguageCode: req.LanguageCode,
            TimeZone: req.TimeZone,
            CurrencyCode: req.CurrencyCode));

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> DeactivateOrg(string orgId, ISender mediator)
    {
        var result = await mediator.Send(new DeactivateOrganisationCommand(orgId));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> ReactivateOrg(string orgId, ISender mediator)
    {
        var result = await mediator.Send(new ReactivateOrganisationCommand(orgId));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }
}
