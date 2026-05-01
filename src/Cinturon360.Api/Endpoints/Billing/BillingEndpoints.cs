using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Abstractions.Services;
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

        group.MapPost("/prepaid/topup", InitiateTopUp)
            .WithName("InitiateTopUp")
            .Produces<ApiResponse<InitiateTopUpResponse>>(200)
            .Produces<ApiResponse<object>>(400);

        // Stripe webhook — no auth, signature verified internally
        app.MapPost("/api/v1/webhooks/stripe", HandleStripeWebhook)
            .WithTags("Webhooks")
            .WithName("StripeWebhook")
            .Produces(204)
            .Produces(400);

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

    private static async Task<IResult> InitiateTopUp(
        string orgId,
        [FromBody] InitiateTopUpRequest request,
        ISender mediator)
    {
        if (request.Amount <= 0)
            return Results.BadRequest(ApiResponse.Fail(new ApiError("billing.invalid_amount", "Amount must be greater than zero.")));

        var result = await mediator.Send(new InitiateTopUpCommand(
            orgId, request.Amount, request.CurrencyCode, request.BillingEmail, request.BillingName));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(new InitiateTopUpResponse(result.Value.PaymentIntentId, result.Value.ClientSecret)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> HandleStripeWebhook(
        HttpRequest httpRequest,
        ISender mediator,
        IPaymentGateway paymentGateway,
        ILoggerFactory loggerFactory)
    {
        // Read raw body — must not use buffering/JSON middleware here
        httpRequest.EnableBuffering();
        using var reader = new global::System.IO.StreamReader(httpRequest.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0;

        var logger = loggerFactory.CreateLogger("StripeWebhook");

        if (!httpRequest.Headers.TryGetValue("Stripe-Signature", out var sig))
            return Results.BadRequest("Missing Stripe-Signature header.");

        var webhookEvent = paymentGateway.ParseWebhookEvent(rawBody, sig!);
        if (webhookEvent is null)
            return Results.BadRequest("Webhook signature verification failed.");

        switch (webhookEvent.EventType)
        {
            case "payment_intent.succeeded":
            {
                if (webhookEvent.OrgId is null || webhookEvent.Amount is null || webhookEvent.Currency is null)
                {
                    logger.LogWarning("payment_intent.succeeded missing orgId/amount/currency metadata — skipping");
                    break;
                }

                await mediator.Send(new ConfirmTopUpCommand(
                    webhookEvent.OrgId,
                    webhookEvent.Amount.Value,
                    webhookEvent.Currency.ToUpperInvariant(),
                    webhookEvent.RelatedObjectId ?? string.Empty));
                break;
            }

            case "payment_intent.payment_failed":
                logger.LogWarning("Stripe payment failed for intent {IntentId}", webhookEvent.RelatedObjectId);
                break;

            case "charge.dispute.created":
                logger.LogWarning("Stripe dispute created for charge {ChargeId}", webhookEvent.RelatedObjectId);
                break;

            default:
                logger.LogDebug("Unhandled Stripe event type: {EventType}", webhookEvent.EventType);
                break;
        }

        return Results.NoContent();
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
