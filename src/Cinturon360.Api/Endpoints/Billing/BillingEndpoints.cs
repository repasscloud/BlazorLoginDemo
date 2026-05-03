using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Application.Features.Billing.Commands;
using Cinturon360.Application.Features.Billing.Queries;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Contracts.Billing;
using Cinturon360.Contracts.Common.Errors;
using Cinturon360.Contracts.Common.Results;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Api.Endpoints.Billing;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/organisations/{orgId}/billing").WithTags("Billing").RequireAuthorization();
        var managementGroup = app.MapGroup("/api/v1/billing").WithTags("Billing Management").RequireAuthorization();

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

        group.MapPost("/relationships/{billingRelationshipId}/prepaid/topup", InitiateRelationshipTopUp)
            .WithName("InitiateRelationshipTopUp")
            .Produces<ApiResponse<RelationshipTopUpResponse>>(200)
            .Produces<ApiResponse<object>>(400);

        managementGroup.MapPost("/provider-connections/stripe", ConfigureStripeProviderConnection)
            .WithName("ConfigureStripeProviderConnection")
            .Produces<ApiResponse<ConfigureStripeProviderConnectionResponse>>(201)
            .Produces<ApiResponse<object>>(400);

        managementGroup.MapPost("/relationships", CreateBillingRelationship)
            .WithName("CreateBillingRelationship")
            .Produces<ApiResponse<CreateBillingRelationshipResponse>>(201)
            .Produces<ApiResponse<object>>(400);

        managementGroup.MapPost("/relationships/{billingRelationshipId}/setup-link", CreateBillingSetupLink)
            .WithName("CreateBillingSetupLink")
            .Produces<ApiResponse<CreateBillingSetupLinkResponse>>(200)
            .Produces<ApiResponse<object>>(400);

        managementGroup.MapGet("/provider-customers/{providerCustomerId}/payment-methods", ListProviderPaymentMethods)
            .WithName("ListProviderPaymentMethods")
            .Produces<ApiResponse<IReadOnlyList<ProviderPaymentMethodResponse>>>(200)
            .Produces<ApiResponse<object>>(404);

        // Stripe webhook — no auth, signature verified internally
        app.MapPost("/api/v1/webhooks/stripe", HandleStripeWebhook)
            .WithTags("Webhooks")
            .WithName("StripeWebhook")
            .Produces(204)
            .Produces(400);

        app.MapPost("/api/v1/webhooks/payment-providers/stripe/{connectionId}", HandleStripeWebhookByConnection)
            .WithTags("Webhooks")
            .WithName("StripeWebhookByConnection")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        app.MapPost("/api/v1/webhooks/stripe/{orgScope}/{orgId}", HandleStripeWebhookByOrgScope)
            .WithTags("Webhooks")
            .WithName("StripeWebhookByOrgScope")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        managementGroup.MapPut("/provider-connections/{connectionId}/primary", SetPrimaryProviderConnection)
            .WithName("SetPrimaryProviderConnection")
            .Produces<ApiResponse<object>>(204)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404);

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

    private static async Task<IResult> InitiateRelationshipTopUp(
        string orgId,
        string billingRelationshipId,
        [FromBody] RelationshipTopUpRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new InitiateRelationshipTopUpCommand(
            billingRelationshipId,
            request.Amount,
            request.CurrencyCode,
            request.BillingEmail,
            request.BillingName));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(new RelationshipTopUpResponse(result.Value.PaymentIntentId, result.Value.ClientSecret)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> ConfigureStripeProviderConnection(
        [FromBody] ConfigureStripeProviderConnectionRequest request,
        ISender mediator)
    {
        var usageScope = Enum.IsDefined(typeof(ProviderUsageScope), request.UsageScope)
            ? (ProviderUsageScope)request.UsageScope
            : ProviderUsageScope.OwnerOnly;

        var result = await mediator.Send(new ConfigureStripeProviderConnectionCommand(
            request.OwnerOrganisationId,
            request.DisplayName,
            request.IsLiveMode,
            request.SecretKey,
            request.PublishableKey,
            request.AllowChildOrgBilling,
            request.AllowClientCheckout,
            request.AllowMonthlyInvoiceCollection,
            usageScope));

        return result.IsSuccess
            ? Results.Created(
                $"/api/v1/billing/provider-connections/{result.Value.ConnectionId}",
                ApiResponse.Ok(new ConfigureStripeProviderConnectionResponse(
                    result.Value.ConnectionId,
                    result.Value.WebhookPath,
                    result.Value.SecretBundleReference,
                    result.Value.IsVerified)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> CreateBillingRelationship(
        [FromBody] CreateBillingRelationshipRequest request,
        ISender mediator)
    {
        if (!Enum.IsDefined(typeof(BillingMode), request.BillingMode)
            || !Enum.IsDefined(typeof(BillingFrequency), request.BillingFrequency)
            || !Enum.IsDefined(typeof(CollectionMode), request.CollectionMode)
            || !Enum.IsDefined(typeof(ChargeTreatment), request.ChargeTreatment)
            || !Enum.IsDefined(typeof(CommercialRiskOwner), request.CommercialRiskOwner))
        {
            return Results.BadRequest(ApiResponse.Fail(new ApiError("billing.invalid_enum", "One or more enum values are invalid.")));
        }

        var result = await mediator.Send(new CreateBillingRelationshipCommand(
            request.SellerOrganisationId,
            request.BuyerOrganisationId,
            request.PaymentProviderConnectionId,
            (BillingMode)request.BillingMode,
            (BillingFrequency)request.BillingFrequency,
            (CollectionMode)request.CollectionMode,
            (ChargeTreatment)request.ChargeTreatment,
            request.CurrencyCode,
            request.PaymentTermsDays,
            request.AutoCollectPayment,
            request.GenerateInvoices,
            request.RequiresPaymentSetup,
            (CommercialRiskOwner)request.CommercialRiskOwner));

        return result.IsSuccess
            ? Results.Created($"/api/v1/billing/relationships/{result.Value}", ApiResponse.Ok(new CreateBillingRelationshipResponse(result.Value)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> CreateBillingSetupLink(
        string billingRelationshipId,
        [FromBody] CreateBillingSetupLinkRequest request,
        ISender mediator)
    {
        if (!Enum.IsDefined(typeof(PaymentMethodPurpose), request.Purpose))
            return Results.BadRequest(ApiResponse.Fail(new ApiError("billing.invalid_purpose", "Invalid payment method purpose.")));

        var result = await mediator.Send(new CreateBillingSetupLinkCommand(
            billingRelationshipId,
            request.RecipientEmail,
            request.RecipientName,
            request.ReturnUrl,
            request.CancelUrl,
            (PaymentMethodPurpose)request.Purpose,
            request.SendEmail));

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(new CreateBillingSetupLinkResponse(result.Value.CheckoutUrl, result.Value.ProviderCustomerId)))
            : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
    }

    private static async Task<IResult> ListProviderPaymentMethods(
        string providerCustomerId,
        ISender mediator)
    {
        var result = await mediator.Send(new ListProviderPaymentMethodsQuery(providerCustomerId));
        if (result.IsFailure)
            return Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));

        var response = result.Value.Select(x => new ProviderPaymentMethodResponse(
            x.Id,
            x.ProviderPaymentMethodId,
            x.DisplayName,
            x.Brand,
            x.Last4,
            x.ExpiryMonth,
            x.ExpiryYear)).ToList();

        return Results.Ok(ApiResponse.Ok<IReadOnlyList<ProviderPaymentMethodResponse>>(response));
    }

    private static async Task<IResult> SetPrimaryProviderConnection(
        string connectionId,
        [FromBody] SetPrimaryProviderConnectionRequest request,
        ISender mediator)
    {
        var result = await mediator.Send(new SetPrimaryProviderConnectionCommand(connectionId, request.OwnerOrganisationId));
        if (result.IsFailure)
        {
            return result.Error.Code == "billing.provider_connection_not_found"
                ? Results.NotFound(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)))
                : Results.BadRequest(ApiResponse.Fail(new ApiError(result.Error.Code, result.Error.Description)));
        }
        return Results.NoContent();
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

    private static async Task<IResult> HandleStripeWebhookByConnection(
        string connectionId,
        HttpRequest httpRequest,
        ISender mediator,
        IBillingRepository billingRepository,
        ISecretStore secretStore,
        IPaymentProviderGateway paymentProviderGateway,
        IUnitOfWork unitOfWork,
        ILoggerFactory loggerFactory)
    {
        var connection = await billingRepository.GetProviderConnectionByIdAsync(connectionId);
        if (connection is null || connection.ProviderType != PaymentProviderType.Stripe || !connection.IsEnabled)
            return Results.NotFound();

        return await ProcessStripeWebhook(connection, httpRequest, mediator, billingRepository, secretStore, paymentProviderGateway, unitOfWork, loggerFactory);
    }

    private static async Task<IResult> HandleStripeWebhookByOrgScope(
        string orgScope,
        string orgId,
        HttpRequest httpRequest,
        ISender mediator,
        IBillingRepository billingRepository,
        IOrganisationRepository organisationRepository,
        ISecretStore secretStore,
        IPaymentProviderGateway paymentProviderGateway,
        IUnitOfWork unitOfWork,
        ILoggerFactory loggerFactory)
    {
        var org = await organisationRepository.GetByIdAsync(orgId);
        if (org is null)
            return Results.NotFound();

        if (!IsOrgScopeMatch(orgScope, org.OrgType))
            return Results.BadRequest("Org scope does not match organisation type.");

        var connection = await billingRepository.GetPrimaryProviderConnectionAsync(orgId);
        if (connection is null)
            return Results.NotFound(new { error = "billing.no_primary_provider_connection", detail = "No verified primary provider connection found for this organisation." });

        return await ProcessStripeWebhook(connection, httpRequest, mediator, billingRepository, secretStore, paymentProviderGateway, unitOfWork, loggerFactory);
    }

    private static async Task<IResult> ProcessStripeWebhook(
        PaymentProviderConnection connection,
        HttpRequest httpRequest,
        ISender mediator,
        IBillingRepository billingRepository,
        ISecretStore secretStore,
        IPaymentProviderGateway paymentProviderGateway,
        IUnitOfWork unitOfWork,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("StripeWebhookV2");

        httpRequest.EnableBuffering();
        using var reader = new global::System.IO.StreamReader(httpRequest.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        httpRequest.Body.Position = 0;

        if (!httpRequest.Headers.TryGetValue("Stripe-Signature", out var signature))
            return Results.BadRequest("Missing Stripe-Signature header.");

        var webhookSecretRef = connection.WebhookSecretReference ?? $"{connection.SecretBundleReference}--webhook-secret";
        string webhookSecret;
        try
        {
            webhookSecret = await secretStore.GetSecretValueAsync(webhookSecretRef, CancellationToken.None);
        }
        catch
        {
            return Results.BadRequest("Webhook secret is not configured for this connection.");
        }

        var parsed = paymentProviderGateway.ParseWebhookEvent(rawBody, signature!, webhookSecret);
        if (parsed is null)
            return Results.BadRequest("Webhook signature verification failed.");

        var existing = await billingRepository.GetProviderWebhookEventAsync(connection.Id, parsed.ProviderEventId);
        if (existing is not null)
            return Results.NoContent();

        var webhookEvent = ProviderWebhookEvent.Create(
            IdGenerator.New(IdPrefix.ProviderWebhookEvent),
            connection.Id,
            PaymentProviderType.Stripe,
            parsed.ProviderEventId,
            parsed.EventType,
            parsed.RawPayloadJson);

        await billingRepository.AddProviderWebhookEventAsync(webhookEvent);

        try
        {
            switch (parsed.EventType)
            {
                case "payment_intent.succeeded":
                {
                    if (!parsed.Metadata.TryGetValue("paymentType", out var paymentType)
                        || !string.Equals(paymentType, "prepaid_topup", StringComparison.OrdinalIgnoreCase))
                    {
                        webhookEvent.MarkProcessed();
                        break;
                    }

                    if (!parsed.Metadata.TryGetValue("billingRelationshipId", out var billingRelationshipId)
                        || !parsed.Metadata.TryGetValue("buyerOrganisationId", out var buyerOrganisationId)
                        || parsed.Amount is null
                        || parsed.Currency is null
                        || string.IsNullOrWhiteSpace(parsed.ProviderObjectId))
                    {
                        webhookEvent.MarkFailed("payment_intent.succeeded missing required metadata.");
                        break;
                    }

                    await mediator.Send(new ConfirmRelationshipTopUpCommand(
                        billingRelationshipId,
                        buyerOrganisationId,
                        parsed.Amount.Value,
                        parsed.Currency.ToUpperInvariant(),
                        parsed.ProviderObjectId));

                    webhookEvent.MarkProcessed();
                    break;
                }

                case "checkout.session.completed":
                case "setup_intent.succeeded":
                case "payment_method.attached":
                {
                    if (!string.IsNullOrWhiteSpace(parsed.ProviderCustomerId))
                    {
                        var providerCustomer = await billingRepository.GetProviderCustomerByProviderIdAsync(
                            connection.Id,
                            parsed.ProviderCustomerId!);

                        if (providerCustomer is not null)
                            await mediator.Send(new ListProviderPaymentMethodsQuery(providerCustomer.Id));
                    }

                    webhookEvent.MarkProcessed();
                    break;
                }

                default:
                    logger.LogDebug("Unhandled Stripe webhook event type: {EventType}", parsed.EventType);
                    webhookEvent.MarkProcessed();
                    break;
            }
        }
        catch (Exception ex)
        {
            webhookEvent.MarkFailed(ex.Message);
            logger.LogError(ex, "Stripe webhook processing failed for connection {ConnectionId}", connection.Id);
        }

        connection.MarkWebhookReceived();
        billingRepository.UpdateProviderWebhookEvent(webhookEvent);
        billingRepository.UpdateProviderConnection(connection);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        return Results.NoContent();
    }

    private static bool IsOrgScopeMatch(string orgScope, OrgType orgType)
        => (orgScope.ToLowerInvariant(), orgType) switch
        {
            ("vendor", OrgType.Vendor) => true,
            ("tmc", OrgType.Tmc) => true,
            ("client", OrgType.Client) => true,
            _ => false
        };

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
