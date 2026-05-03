using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Application.Features.Billing.Commands;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Enums.Billing;

namespace Cinturon360.Application.Features.Billing.Queries;

public sealed record ListProviderPaymentMethodsQuery(string ProviderCustomerId)
    : IRequest<Result<IReadOnlyList<ProviderPaymentMethod>>>;

public sealed class ListProviderPaymentMethodsHandler(
    IBillingRepository billingRepository,
    ISecretStore secretStore,
    IPaymentProviderGateway paymentProviderGateway,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ListProviderPaymentMethodsQuery, Result<IReadOnlyList<ProviderPaymentMethod>>>
{
    public async Task<Result<IReadOnlyList<ProviderPaymentMethod>>> Handle(ListProviderPaymentMethodsQuery request, CancellationToken ct)
    {
        var providerCustomer = await billingRepository.GetProviderCustomerByIdAsync(request.ProviderCustomerId, ct);
        if (providerCustomer is null)
            return Result.Failure<IReadOnlyList<ProviderPaymentMethod>>(new("billing.provider_customer_not_found", "Provider customer was not found."));

        var connection = await billingRepository.GetProviderConnectionByIdAsync(providerCustomer.PaymentProviderConnectionId, ct);
        if (connection is null)
            return Result.Failure<IReadOnlyList<ProviderPaymentMethod>>(new("billing.provider_connection_not_found", "Provider connection was not found."));

        var context = await BillingProviderCommandHelpers.BuildContextAsync(connection, secretStore, ct);
        var snapshots = await paymentProviderGateway.ListPaymentMethodsAsync(context, providerCustomer.ProviderCustomerId, ct);

        var hasUsable = snapshots.Count > 0;

        foreach (var snapshot in snapshots)
        {
            var existing = await billingRepository.GetProviderPaymentMethodAsync(providerCustomer.Id, snapshot.ProviderPaymentMethodId, ct);
            if (existing is null)
            {
                var paymentMethod = ProviderPaymentMethod.Create(
                    IdGenerator.New(IdPrefix.ProviderPaymentMethod),
                    providerCustomer.Id,
                    PaymentProviderType.Stripe,
                    snapshot.ProviderPaymentMethodId,
                    PaymentMethodPurpose.General,
                    snapshot.DisplayName,
                    snapshot.Brand,
                    snapshot.Last4,
                    snapshot.ExpiryMonth,
                    snapshot.ExpiryYear,
                    snapshot.CardholderName,
                    snapshot.Fingerprint);

                await billingRepository.AddProviderPaymentMethodAsync(paymentMethod, ct);
            }
            else
            {
                existing.SyncDisplay(snapshot.DisplayName, snapshot.Brand, snapshot.Last4, snapshot.ExpiryMonth, snapshot.ExpiryYear);
                billingRepository.UpdateProviderPaymentMethod(existing);
            }
        }

        providerCustomer.MarkSetupComplete(hasUsable);
        billingRepository.UpdateProviderCustomer(providerCustomer);

        var profile = await billingRepository.GetOrganisationBillingProfileAsync(providerCustomer.BuyerOrganisationId, ct);
        if (profile is not null && hasUsable)
        {
            var firstMethod = await billingRepository.ListProviderPaymentMethodsAsync(providerCustomer.Id, ct);
            var defaultMethod = firstMethod.FirstOrDefault();
            profile.SetDefaultPaymentMethod(providerCustomer.Id, defaultMethod?.Id);
            billingRepository.UpdateOrganisationBillingProfile(profile);
        }

        await unitOfWork.SaveChangesAsync(ct);
        var list = await billingRepository.ListProviderPaymentMethodsAsync(providerCustomer.Id, ct);
        return Result.Success<IReadOnlyList<ProviderPaymentMethod>>(list);
    }
}
