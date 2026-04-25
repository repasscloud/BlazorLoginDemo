using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Features.Billing.Commands;
using Cinturon360.Application.Features.Billing.Queries;
using Cinturon360.Contracts.Billing;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Api.Endpoints.Billing;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/organisations/{orgId}/billing").WithTags("Billing").RequireAuthorization();

        group.MapGet("/license", GetLicense)
            .WithName("GetOrgLicense")
            .Produces<ApiResponse<OrgLicenseResponse>>(200)
            .Produces<ApiResponse<object>>(404);

        group.MapGet("/invoices", ListInvoices)
            .WithName("ListInvoices")
            .Produces<ApiResponse<IEnumerable<InvoiceResponse>>>(200);

        group.MapPost("/invoices", CreateInvoice)
            .WithName("CreateInvoice")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/payments", RecordPayment)
            .WithName("RecordPayment")
            .Produces<ApiResponse<string>>(201)
            .Produces<ApiResponse<object>>(400);

        group.MapPost("/prepaid/credit", CreditPrepaid)
            .WithName("CreditPrepaidBalance")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400);

        return app;
    }

    private static async Task<IResult> GetLicense(string orgId, ISender mediator)
    {
        var result = await mediator.Send(new GetOrgLicenseQuery(orgId));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return result.Value is null
            ? Results.NotFound(ApiResponse.Fail(new ApiError("billing.license_not_found", "Organisation license not found.")))
            : Results.Ok(ApiResponse.Ok(MapLicense(result.Value)));
    }

    private static async Task<IResult> ListInvoices(
        string orgId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISender mediator)
    {
        var safePage = page <= 0 ? 1 : page;
        var safeSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

        var result = await mediator.Send(new ListInvoicesQuery(orgId, safePage, safeSize));
        if (result.IsFailure)
            return Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        return Results.Ok(ApiResponse.Ok(result.Value.Select(MapInvoice)));
    }

    private static async Task<IResult> CreateInvoice(string orgId, [FromBody] CreateInvoiceRequest request, ISender mediator)
    {
        var result = await mediator.Send(new CreateInvoiceCommand(
            orgId,
            request.InvoiceNumber,
            request.Subtotal,
            request.Tax,
            request.CurrencyCode,
            request.IssuedOn,
            request.DueOn,
            request.StripeInvoiceId));

        return result.IsSuccess
            ? Results.Created($"/api/v1/organisations/{orgId}/billing/invoices/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> RecordPayment(string orgId, [FromBody] RecordPaymentRequest request, ISender mediator)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), request.Method))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("billing.invalid_payment_method", "Invalid payment method.")));

        var result = await mediator.Send(new RecordPaymentCommand(
            orgId,
            request.Amount,
            request.CurrencyCode,
            (PaymentMethod)request.Method,
            request.InvoiceId,
            request.StripePaymentIntentId));

        return result.IsSuccess
            ? Results.Created($"/api/v1/organisations/{orgId}/billing/payments/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> CreditPrepaid(string orgId, [FromBody] CreditPrepaidBalanceRequest request, ISender mediator)
    {
        var result = await mediator.Send(new CreditPrepaidBalanceCommand(orgId, request.Amount, request.CurrencyCode));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static OrgLicenseResponse MapLicense(OrgLicense l) => new(
        l.Id,
        l.OrgId,
        (int)l.LicenseType,
        (int)l.BillingCycle,
        l.MaxUsers,
        l.MaxBookingsPerMonth,
        l.StartsOn,
        l.ExpiresOn,
        l.IsActive);

    private static InvoiceResponse MapInvoice(Invoice i) => new(
        i.Id,
        i.OrgId,
        i.InvoiceNumber,
        (int)i.Status,
        i.SubtotalAmount,
        i.TaxAmount,
        i.TotalAmount,
        i.CurrencyCode,
        i.IssuedOn,
        i.DueOn,
        i.PaidAt);
}
