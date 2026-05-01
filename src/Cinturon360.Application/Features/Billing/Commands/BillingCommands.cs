using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Application.Features.Billing.Commands;

// ── Errors ─────────────────────────────────────────────────────────────────
public static class BillingErrors
{
    public static readonly Error LicenseNotFound  = new("billing.license_not_found",  "Organisation license not found.");
    public static readonly Error InvoiceNotFound  = new("billing.invoice_not_found",  "Invoice not found.");
    public static readonly Error PaymentNotFound  = new("billing.payment_not_found",  "Payment not found.");
    public static readonly Error BalanceNotFound  = new("billing.balance_not_found",  "Prepaid balance not found.");
}

// ── Create invoice ────────────────────────────────────────────────────────
public sealed record CreateInvoiceCommand(
    string OrgId,
    string InvoiceNumber,
    decimal Subtotal,
    decimal Tax,
    string CurrencyCode,
    DateOnly IssuedOn,
    DateOnly DueOn,
    string? StripeInvoiceId) : IRequest<Result<string>>;

public sealed class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<string>>
{
    private readonly IBillingRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateInvoiceHandler(IBillingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Invoice);
        var invoice = Invoice.Create(id, request.OrgId, request.InvoiceNumber,
            request.Subtotal, request.Tax, request.CurrencyCode,
            request.IssuedOn, request.DueOn, request.StripeInvoiceId);

        await _repo.AddInvoiceAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Record payment ────────────────────────────────────────────────────────
public sealed record RecordPaymentCommand(
    string OrgId,
    decimal Amount,
    string CurrencyCode,
    PaymentMethod Method,
    string? InvoiceId,
    string? StripePaymentIntentId) : IRequest<Result<string>>;

public sealed class RecordPaymentHandler : IRequestHandler<RecordPaymentCommand, Result<string>>
{
    private readonly IBillingRepository _repo;
    private readonly IUnitOfWork _uow;

    public RecordPaymentHandler(IBillingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var id = IdGenerator.New(IdPrefix.Payment);
        var payment = Payment.Create(id, request.OrgId, request.Amount, request.CurrencyCode,
            request.Method, request.InvoiceId, request.StripePaymentIntentId);

        await _repo.AddPaymentAsync(payment, ct);

        // Mark corresponding invoice as paid if specified
        if (request.InvoiceId is not null)
        {
            var invoice = await _repo.GetInvoiceByIdAsync(request.InvoiceId, ct);
            if (invoice is not null)
            {
                invoice.MarkPaid();
                _repo.UpdateInvoice(invoice);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success(id);
    }
}

// ── Credit prepaid balance ────────────────────────────────────────────────
public sealed record CreditPrepaidBalanceCommand(
    string OrgId,
    decimal Amount,
    string CurrencyCode) : IRequest<Result>;

public sealed class CreditPrepaidBalanceHandler : IRequestHandler<CreditPrepaidBalanceCommand, Result>
{
    private readonly IBillingRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreditPrepaidBalanceHandler(IBillingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(CreditPrepaidBalanceCommand request, CancellationToken ct)
    {
        var balance = await _repo.GetPrepaidBalanceAsync(request.OrgId, ct);
        if (balance is null)
        {
            var newId = IdGenerator.New(IdPrefix.PrepaidBalance);
            balance = PrepaidBalance.Create(newId, request.OrgId, request.CurrencyCode);
            balance.Credit(request.Amount);
            await _repo.AddPrepaidBalanceAsync(balance, ct);
        }
        else
        {
            balance.Credit(request.Amount);
            _repo.UpdatePrepaidBalance(balance);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Initiate prepaid top-up via Stripe ────────────────────────────────────
/// <summary>
/// Creates a Stripe PaymentIntent for a prepaid top-up. Returns the client secret
/// so the frontend can complete payment with Stripe.js.
/// Call <see cref="ConfirmTopUpCommand"/> from the Stripe webhook after payment succeeds.
/// </summary>
public sealed record InitiateTopUpCommand(
    string OrgId,
    decimal Amount,
    string CurrencyCode,
    string BillingEmail,
    string BillingName) : IRequest<Result<InitiateTopUpResult>>;

public sealed record InitiateTopUpResult(string PaymentIntentId, string ClientSecret);

public sealed class InitiateTopUpHandler(
    IBillingRepository repo,
    IPaymentGateway paymentGateway,
    IUnitOfWork uow,
    ILogger<InitiateTopUpHandler> logger) : IRequestHandler<InitiateTopUpCommand, Result<InitiateTopUpResult>>
{
    public async Task<Result<InitiateTopUpResult>> Handle(InitiateTopUpCommand request, CancellationToken ct)
    {
        var billingConfig = await repo.GetBillingConfigAsync(request.OrgId, ct);

        string customerId;

        if (billingConfig?.StripeCustomerId is not null)
        {
            customerId = billingConfig.StripeCustomerId;
        }
        else
        {
            var customerResult = await paymentGateway.CreateCustomerAsync(
                request.OrgId, request.BillingEmail, request.BillingName, ct);

            if (!customerResult.Success || customerResult.CustomerId is null)
            {
                logger.LogError("Failed to create Stripe customer for org {OrgId}: {Error}", request.OrgId, customerResult.Error);
                return Result.Failure<InitiateTopUpResult>(new("billing.stripe_customer_failed", "Failed to create Stripe customer."));
            }

            customerId = customerResult.CustomerId;

            if (billingConfig is null)
            {
                var configId = IdGenerator.New(IdPrefix.PrepaidBalance); // reuse prefix for config ID
                billingConfig = OrgBillingConfig.Create(configId, request.OrgId,
                    stripeCustomerId: customerId,
                    billingEmail: request.BillingEmail,
                    billingName: request.BillingName,
                    currencyCode: request.CurrencyCode);
                await repo.AddBillingConfigAsync(billingConfig, ct);
            }
            else
            {
                billingConfig.SetStripeCustomerId(customerId);
                repo.UpdateBillingConfig(billingConfig);
            }

            await uow.SaveChangesAsync(ct);
        }

        var intentResult = await paymentGateway.CreatePaymentIntentAsync(
            customerId, request.Amount, request.CurrencyCode,
            $"Prepaid top-up for org {request.OrgId}", ct);

        if (!intentResult.Success || intentResult.PaymentIntentId is null || intentResult.ClientSecret is null)
        {
            logger.LogError("Failed to create Stripe payment intent for org {OrgId}: {Error}", request.OrgId, intentResult.Error);
            return Result.Failure<InitiateTopUpResult>(new("billing.stripe_intent_failed", "Failed to create payment intent."));
        }

        return Result.Success(new InitiateTopUpResult(intentResult.PaymentIntentId, intentResult.ClientSecret));
    }
}

// ── Confirm top-up after Stripe webhook ──────────────────────────────────
/// <summary>
/// Called by the Stripe webhook handler when payment_intent.succeeded fires.
/// Credits the prepaid balance and records the payment.
/// </summary>
public sealed record ConfirmTopUpCommand(
    string OrgId,
    decimal Amount,
    string CurrencyCode,
    string StripePaymentIntentId) : IRequest<Result>;

public sealed class ConfirmTopUpHandler(
    IBillingRepository repo,
    IUnitOfWork uow) : IRequestHandler<ConfirmTopUpCommand, Result>
{
    public async Task<Result> Handle(ConfirmTopUpCommand request, CancellationToken ct)
    {
        // Credit prepaid balance
        var balance = await repo.GetPrepaidBalanceAsync(request.OrgId, ct);
        if (balance is null)
        {
            var balanceId = IdGenerator.New(IdPrefix.PrepaidBalance);
            balance = PrepaidBalance.Create(balanceId, request.OrgId, request.CurrencyCode);
            balance.Credit(request.Amount);
            await repo.AddPrepaidBalanceAsync(balance, ct);
        }
        else
        {
            balance.Credit(request.Amount);
            repo.UpdatePrepaidBalance(balance);
        }

        // Record payment
        var paymentId = IdGenerator.New(IdPrefix.Payment);
        var payment = Payment.Create(paymentId, request.OrgId, request.Amount, request.CurrencyCode,
            PaymentMethod.Card, invoiceId: null, request.StripePaymentIntentId);
        await repo.AddPaymentAsync(payment, ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
