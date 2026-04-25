using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
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
