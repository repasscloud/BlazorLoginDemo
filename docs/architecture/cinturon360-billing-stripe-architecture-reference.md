# Cinturon360 Billing, Stripe, Payment Provider, and Organisation Billing Architecture

## Purpose

This document defines how Cinturon360 billing and Stripe payment functionality should be redesigned and implemented.

The previous Stripe implementation/runbook only described a narrow prepaid top-up flow. That is not sufficient for the actual Cinturon360 commercial model.

Cinturon360 must support:

- Vendor → TMC → Client organisation hierarchy;
- Vendor → Vendor → TMC → Client hierarchies;
- parent organisations onboarding child organisations;
- billing relationships that are separate from organisation hierarchy;
- each seller organisation optionally using its own payment provider account;
- Stripe as one supported provider, not the billing source of truth;
- saved payment methods on buyer/customer accounts;
- different payment methods for different travel policies;
- immediate payments, delayed billing, scheduled billing, deposits, future balances, authorise-only/capture-later, manual billing and external billing;
- Azure Key Vault secret storage for provider credentials;
- provider-specific webhooks routed back into Cinturon360;
- Cinturon360-owned billing ledger, invoice, charge-line and audit records;
- external finance/accounting exports.

This document is intended as an implementation reference for the Cinturon360 platform.

---

## Core Principle

Cinturon360 must own the billing logic.

Payment providers such as Stripe, Airwallex, Braintree, PayPal and Square are execution channels only.

Cinturon360 should be the source of truth for:

- organisation hierarchy;
- seller/buyer billing relationships;
- commercial billing responsibility;
- organisation licences;
- billing rules;
- travel policy payment rules;
- booking finance;
- deposits;
- scheduled payment items;
- service fees;
- tax/fee/adjustment lines;
- shadow billing;
- invoice generation;
- payment state;
- provider references;
- audit history;
- reporting;
- finance exports.

Stripe should not be the source of truth for Cinturon360 billing logic.

Stripe should receive the final payable representation when Stripe is the selected payment provider.

---

## Terminology

### Platform Organisation

The Avanōa entity operating Cinturon360.

References to `Avanōa Technology` should be treated as interchangeable with whichever Avanōa entity is designated as the driving organisation for Cinturon360, including Avanōa Services, Avanōa Technology, Avanōa Travel, or another nominated Avanōa company.

### Vendor Organisation

A reseller/region/location/geographic account holder.

A Vendor can onboard:

- another Vendor;
- a TMC.

A Vendor may act as a billing owner for child Vendors or TMCs.

### TMC Organisation

A Travel Management Company.

A TMC is usually onboarded under a Vendor. A TMC can onboard Client organisations.

A TMC can be the seller/billing owner for its Client organisations.

### Client Organisation

A corporate/client customer of a TMC.

A Client organisation contains travellers, travel policies, cost centres, billing setup and payment methods.

### Seller Organisation

The organisation that bills another organisation.

Examples:

- Avanōa Technology bills HelloWorld Travel.
- HelloWorld Travel bills Coca-Cola.

### Buyer Organisation

The organisation being billed.

Examples:

- HelloWorld Travel is the buyer when Avanōa Technology bills HelloWorld Travel.
- Coca-Cola is the buyer when HelloWorld Travel bills Coca-Cola.

### Payment Provider Connection

A configured connection between an organisation and a payment provider.

Example:

- HelloWorld Travel's own Stripe account;
- Avanōa Technology's own Stripe account;
- a future Airwallex/Braintree/PayPal/Square connection.

### Provider Customer

A customer record inside the seller organisation's payment provider account.

Example:

- Coca-Cola is a Stripe Customer inside HelloWorld Travel's Stripe account.
- HelloWorld Travel is a Stripe Customer inside Avanōa Technology's Stripe account.

### Provider Payment Method

A saved card/bank/payment method reference inside the provider.

For Stripe, this is the `pm_...` PaymentMethod ID.

---

## Organisation Hierarchy

The organisation hierarchy defines operational ownership, visibility, provisioning and support.

Supported hierarchy examples:

```text
Vendor Org
  -> TMC Org
      -> Client Org
```

```text
Vendor Org
  -> Vendor Org
      -> TMC Org
          -> Client Org
```

```text
Vendor Org
  -> Vendor Org
      -> Vendor Org
          -> TMC Org
              -> Client Org
```

The hierarchy should not automatically determine billing responsibility.

Billing must be modelled separately.

---

## Billing Relationship

A billing relationship defines who bills whom.

Examples:

```text
Seller: Avanōa Technology
Buyer: HelloWorld Travel
Provider: Avanōa Technology Stripe
```

```text
Seller: HelloWorld Travel
Buyer: Coca-Cola
Provider: HelloWorld Travel Stripe
```

This must be separate from the parent/child organisation tree because commercial arrangements can vary.

---

## Commercial Risk Boundary

Where a parent organisation configures billing for a child organisation, the parent organisation is the seller/billing owner for that relationship.

Any credit exposure, payment terms, delayed billing arrangement, manual collection process, external settlement, or buyer non-payment risk belongs to the seller organisation.

Example:

```text
Avanōa Technology -> HelloWorld Travel -> Coca-Cola
```

Commercial relationships:

```text
Avanōa Technology bills HelloWorld Travel.
HelloWorld Travel bills Coca-Cola.
```

If Coca-Cola is configured for delayed monthly billing by HelloWorld Travel:

- HelloWorld Travel owns the credit risk.
- HelloWorld Travel owns the commercial relationship.
- HelloWorld Travel decides whether to allow delayed billing.
- Avanōa Technology does not underwrite Coca-Cola.
- Avanōa Technology does not collect those funds unless Avanōa Technology is the seller.
- Avanōa Technology is only executing the configured platform workflow.

Avanōa Technology should be responsible for:

- platform availability;
- correct charge calculation;
- correct routing to the configured payment provider;
- recording invoices and payments accurately;
- applying configured billing rules;
- audit logging;
- access control.

Avanōa Technology should not be responsible for:

- whether Coca-Cola pays HelloWorld Travel;
- HelloWorld Travel extending credit to Coca-Cola;
- HelloWorld Travel choosing delayed billing;
- HelloWorld Travel choosing manual/external settlement;
- disputes between HelloWorld Travel and Coca-Cola.

---

## Required Billing Modes

Cinturon360 must support more than prepaid top-ups.

Recommended billing modes:

```csharp
public enum BillingMode
{
    NoCharge = 0,

    // Buyer prepays credit, then usage consumes the balance.
    PrepaidBalance = 1,

    // Payment is collected immediately per transaction.
    ImmediatePayment = 2,

    // Charges accumulate and are billed periodically.
    PeriodicInvoice = 3,

    // C360 generates invoice but payment is collected manually.
    ManualInvoice = 4,

    // Billing is recorded but handled outside C360.
    ExternalBilling = 5,

    // Parent/seller manages billing for child/buyer.
    PartnerManaged = 6
}
```

Recommended payment collection modes:

```csharp
public enum PaymentCollectionMode
{
    None = 0,
    Manual = 1,
    Automatic = 2,
    ExternalReferenceOnly = 3
}
```

Recommended charge treatment:

```csharp
public enum ChargeTreatment
{
    Billable = 0,

    // Calculate but do not issue payable invoice.
    ShadowOnly = 1,

    // Charge exists but is waived by rule/commercial decision.
    Waived = 2
}
```

Recommended billing frequency:

```csharp
public enum BillingFrequency
{
    None = 0,
    PerTransaction = 1,
    Daily = 2,
    Weekly = 3,
    Fortnightly = 4,
    Monthly = 5,
    CustomCron = 6
}
```

---

## Supported Payment Provider Types

Cinturon360 should be provider-neutral.

```csharp
public enum PaymentProviderType
{
    None = 0,
    Stripe = 1,
    Airwallex = 2,
    Braintree = 3,
    PayPal = 4,
    Square = 5,
    Manual = 6,
    External = 7
}
```

Stripe should be one implementation of a generic payment provider abstraction.

---

## Payment Provider Credentials

### Rule

The database must never store raw provider API keys, secrets, webhook signing secrets, access tokens, private keys or client secrets.

The database stores:

- provider type;
- owner organisation;
- mode/test-live flag;
- connection status;
- safe display metadata;
- provider account ID/name where safe;
- Key Vault secret references;
- webhook endpoint ID/reference;
- allowed usage scope;
- verification timestamps;
- last-used timestamps;
- audit references.

Actual provider secrets must be stored in Azure Key Vault or equivalent.

### Azure Key Vault

Cinturon360 WebAPI should use Azure Key Vault via `Azure.Security.KeyVault.Secrets` and `Azure.Identity`.

The WebAPI should run with managed identity in Azure.

Required packages:

```bash
dotnet add package Azure.Security.KeyVault.Secrets
dotnet add package Azure.Identity
```

The API managed identity should have minimal required secret permissions:

- `get`;
- `set`;
- `update`;
- `delete` if provider removal is self-service;
- avoid granting `purge` to the application identity.

Secret values should be set via API. Updating an existing Key Vault secret name creates a new secret version.

### Secret Naming

Azure Key Vault secret names should not be path-like. Use a deterministic flattened format.

Example:

```text
payment-providers--{ownerOrganisationId:N}--stripe--live--secret-key
payment-providers--{ownerOrganisationId:N}--stripe--live--publishable-key
payment-providers--{ownerOrganisationId:N}--stripe--live--webhook-secret
```

The DB should store a base bundle reference:

```text
payment-providers--{ownerOrganisationId:N}--stripe--live
```

Then derive individual secret names from the bundle reference.

### Secret Store Interface

```csharp
public interface ISecretStore
{
    Task<string> SetSecretAsync(
        string name,
        string value,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken);

    Task<string> GetSecretValueAsync(
        string name,
        CancellationToken cancellationToken);

    Task DisableSecretAsync(
        string name,
        CancellationToken cancellationToken);

    Task DeleteSecretAsync(
        string name,
        CancellationToken cancellationToken);
}
```

### Azure Key Vault Implementation Example

```csharp
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

public sealed class AzureKeyVaultSecretStore : ISecretStore
{
    private readonly SecretClient _client;

    public AzureKeyVaultSecretStore(IConfiguration configuration)
    {
        var vaultUri = configuration["KeyVault:VaultUri"]
            ?? throw new InvalidOperationException("Missing KeyVault:VaultUri.");

        _client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
    }

    public async Task<string> SetSecretAsync(
        string name,
        string value,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken)
    {
        var secret = new KeyVaultSecret(name, value)
        {
            Properties =
            {
                ContentType = "application/x-cinturon360-secret"
            }
        };

        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                secret.Properties.Tags[tag.Key] = tag.Value;
            }
        }

        var saved = await _client.SetSecretAsync(secret, cancellationToken);
        return saved.Id.ToString();
    }

    public async Task<string> GetSecretValueAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var secret = await _client.GetSecretAsync(name, cancellationToken: cancellationToken);
        return secret.Value;
    }

    public async Task DisableSecretAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var secret = await _client.GetSecretAsync(name, cancellationToken: cancellationToken);

        secret.Properties.Enabled = false;

        await _client.UpdateSecretPropertiesAsync(secret.Properties, cancellationToken);
    }

    public async Task DeleteSecretAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var operation = await _client.StartDeleteSecretAsync(name, cancellationToken);

        // Do not purge here. Soft-delete retention should protect recovery.
        await operation.WaitForCompletionAsync(cancellationToken);
    }
}
```

---

## Payment Provider Connection

A seller organisation can configure its own provider connection.

Example:

- Avanōa Technology configures its Stripe account.
- HelloWorld Travel configures its Stripe account.
- Later, another TMC configures Airwallex or PayPal.

Recommended entity:

```csharp
public sealed class PaymentProviderConnection
{
    public Guid Id { get; set; }

    // The organisation that owns the provider account.
    public Guid OwnerOrganisationId { get; set; }

    public PaymentProviderType ProviderType { get; set; }

    public string DisplayName { get; set; } = "";

    public bool IsLiveMode { get; set; }
    public bool IsEnabled { get; set; }

    public ProviderConnectionStatus Status { get; set; }

    // Base reference used to resolve provider secrets in Key Vault.
    public string SecretBundleReference { get; set; } = "";

    // Safe provider account metadata.
    public string? ProviderAccountId { get; set; }
    public string? ProviderAccountName { get; set; }

    // Webhook metadata.
    public string? WebhookEndpointId { get; set; }
    public string? WebhookSecretReference { get; set; }

    public ProviderUsageScope UsageScope { get; set; }

    public bool AllowChildOrgBilling { get; set; }
    public bool AllowClientCheckout { get; set; }
    public bool AllowMonthlyInvoiceCollection { get; set; }

    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? LastRotatedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? LastWebhookReceivedAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
}
```

Recommended statuses:

```csharp
public enum ProviderConnectionStatus
{
    PendingSetup = 0,
    PendingVerification = 1,
    Verified = 2,
    FailedVerification = 3,
    Disabled = 4
}
```

Recommended usage scope:

```csharp
public enum ProviderUsageScope
{
    OwnerOnly = 0,
    DirectChildren = 1,
    Descendants = 2
}
```

---

## Billing Relationship Entity

The billing relationship controls how a buyer is billed by a seller.

```csharp
public sealed class BillingRelationship
{
    public Guid Id { get; set; }

    public Guid SellerOrganisationId { get; set; }
    public Guid BuyerOrganisationId { get; set; }

    public Guid? OrgLicenseId { get; set; }

    public Guid? PaymentProviderConnectionId { get; set; }

    public BillingMode BillingMode { get; set; }
    public BillingFrequency BillingFrequency { get; set; }
    public PaymentCollectionMode CollectionMode { get; set; }
    public ChargeTreatment ChargeTreatment { get; set; }

    public bool GenerateInvoices { get; set; }
    public bool AutoCollectPayment { get; set; }
    public bool RequiresPaymentSetup { get; set; }
    public bool IsBillingSetupComplete { get; set; }

    public string CurrencyCode { get; set; } = "AUD";

    public int PaymentTermsDays { get; set; }

    public decimal? CreditLimitAmount { get; set; }
    public decimal? MaxSingleBookingAmount { get; set; }
    public bool BlockBookingsWhenOverdue { get; set; }
    public bool RequirePaymentBeforeTicketing { get; set; }

    public CommercialRiskOwner CommercialRiskOwner { get; set; }

    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }
}
```

```csharp
public enum CommercialRiskOwner
{
    Platform = 0,
    SellerOrganisation = 1,
    External = 2
}
```

For most TMC → Client relationships:

```text
CommercialRiskOwner = SellerOrganisation
SellerOrganisationId = TMC
BuyerOrganisationId = Client
```

---

## Provider Customer

Do not store a single `StripeCustomerId` directly on `Organisation`.

The same organisation can be a customer under different seller/provider accounts.

Correct model:

```csharp
public sealed class ProviderCustomer
{
    public Guid Id { get; set; }

    // The seller's provider connection.
    public Guid PaymentProviderConnectionId { get; set; }

    // The org being billed.
    public Guid BuyerOrganisationId { get; set; }

    // Stripe cus_..., or equivalent provider customer reference.
    public string ProviderCustomerId { get; set; } = "";

    public bool IsEnabled { get; set; }

    public bool HasUsablePaymentMethod { get; set; }

    public DateTimeOffset? SetupCompletedAt { get; set; }
    public DateTimeOffset? LastPaymentSucceededAt { get; set; }
    public DateTimeOffset? LastPaymentFailedAt { get; set; }
}
```

Example:

```text
PaymentProviderConnection = HelloWorld Travel Stripe
BuyerOrganisation = Coca-Cola
ProviderCustomerId = cus_URBGtGUhwTKSf4
```

Stripe customer IDs (`cus_...`) must be stored because all Stripe charges/invoices/payment methods must be associated with the correct customer.

---

## Provider Payment Method

For Stripe, the card ID/reference Cinturon360 must store is the PaymentMethod ID:

```text
pm_...
```

Example from Stripe:

```text
American Express •••• 8431
ID: pm_1TSIxN4BF2uOrrJbosRoVi2I
Reusable Payment ID / SetupIntent: seti_...
```

Cinturon360 should use/store:

```text
pm_1TSIxN4BF2uOrrJbosRoVi2I
```

The `seti_...` SetupIntent ID may be stored for audit, but not for charging.

Recommended entity:

```csharp
public sealed class ProviderPaymentMethod
{
    public Guid Id { get; set; }

    public Guid ProviderCustomerId { get; set; }

    public PaymentProviderType ProviderType { get; set; }

    // Stripe pm_..., or equivalent provider payment method reference.
    public string ProviderPaymentMethodId { get; set; } = "";

    // Optional setup/audit reference, e.g. Stripe seti_...
    public string? ProviderSetupIntentId { get; set; }

    public PaymentMethodPurpose Purpose { get; set; }

    public string DisplayName { get; set; } = "";

    // Safe display metadata only.
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }

    public string? CardholderName { get; set; }
    public string? Fingerprint { get; set; }

    public string? BillingCountry { get; set; }
    public string? BillingEmail { get; set; }

    public bool IsDefaultForAccountFees { get; set; }
    public bool IsEnabled { get; set; }

    public DateTimeOffset AddedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
}
```

```csharp
public enum PaymentMethodPurpose
{
    General = 0,
    AccountFees = 1,
    TravelBookings = 2,
    TravelPolicy = 3,
    EmergencyFallback = 4
}
```

Cinturon360 may store safe display metadata such as:

- brand;
- last 4;
- expiry month/year;
- cardholder name;
- billing country;
- fingerprint if useful for duplicate detection.

Cinturon360 must not store:

- full card number;
- CVC;
- raw bank account credentials.

---

## Stripe Customer Portal / Payment Method Management

Customer-facing Stripe-hosted flows typically allow customers to:

- add a payment method;
- remove a payment method;
- set default payment method.

Customers generally do not edit an existing card in-place. They add a replacement card, set it as default if required, then remove the old card.

Cinturon360 UX should reflect this.

Use:

- Add payment method;
- Set as organisation default;
- Assign to travel policy;
- Remove/disable payment method;
- Refresh from provider.

Avoid:

- Edit card number;
- Edit CVC;
- Modify card details.

---

## Payment Method Setup Flow

A buyer organisation finance/admin user should be able to add payment methods without exposing card details to Cinturon360.

Example:

```text
CCA Finance user logs into C360.
User opens Billing Setup.
User clicks Add payment method.
C360 creates Stripe Checkout Session in setup mode, or SetupIntent.
User enters card/bank details in Stripe-hosted UI.
Stripe stores payment method against CCA Customer inside HwTravel's Stripe account.
Stripe redirects user back to C360.
Stripe sends webhook confirmation.
C360 stores ProviderPaymentMethod reference and safe metadata.
```

Stripe setup mode/SetupIntent is used to save payment details for future reuse without taking immediate payment.

Recommended endpoint:

```http
POST /api/v1/organisations/{organisationId}/billing/payment-method-setup-session
```

Request:

```json
{
  "billingRelationshipId": "7bfca46f-0d41-4fa6-9db2-3f9111572ac4",
  "purpose": "TravelPolicy",
  "returnUrl": "https://app.cinturon360.com/client/billing/payment-methods",
  "cancelUrl": "https://app.cinturon360.com/client/billing/payment-methods"
}
```

Response:

```json
{
  "checkoutUrl": "https://checkout.stripe.com/c/..."
}
```

Implementation must use the seller organisation's provider connection.

Example:

```text
Seller = HelloWorld Travel
Buyer = Coca-Cola
Provider connection = HelloWorld Travel Stripe
Stripe Customer = Coca-Cola customer inside HelloWorld Travel Stripe
```

---

## Stripe Setup Session Implementation Example

```csharp
public sealed record CreatePaymentMethodSetupSessionRequest(
    Guid BillingRelationshipId,
    PaymentMethodPurpose Purpose,
    string ReturnUrl,
    string CancelUrl);

public sealed record CreatePaymentMethodSetupSessionResult(
    string CheckoutUrl);
```

Pseudo-implementation:

```csharp
public async Task<CreatePaymentMethodSetupSessionResult> CreateStripeSetupSessionAsync(
    CreatePaymentMethodSetupSessionRequest request,
    CancellationToken cancellationToken)
{
    var relationship = await db.BillingRelationships
        .SingleAsync(x => x.Id == request.BillingRelationshipId, cancellationToken);

    var connection = await db.PaymentProviderConnections
        .SingleAsync(x => x.Id == relationship.PaymentProviderConnectionId, cancellationToken);

    var providerCustomer = await EnsureProviderCustomerAsync(
        connection,
        relationship.BuyerOrganisationId,
        cancellationToken);

    var stripeSecretKey = await secretStore.GetSecretValueAsync(
        $"{connection.SecretBundleReference}--secret-key",
        cancellationToken);

    var sessionOptions = new Stripe.Checkout.SessionCreateOptions
    {
        Mode = "setup",
        Customer = providerCustomer.ProviderCustomerId,
        SuccessUrl = request.ReturnUrl,
        CancelUrl = request.CancelUrl,
        PaymentMethodTypes = ["card"],
        Metadata = new Dictionary<string, string>
        {
            ["billingRelationshipId"] = relationship.Id.ToString(),
            ["sellerOrganisationId"] = relationship.SellerOrganisationId.ToString(),
            ["buyerOrganisationId"] = relationship.BuyerOrganisationId.ToString(),
            ["purpose"] = request.Purpose.ToString()
        }
    };

    var sessionService = new Stripe.Checkout.SessionService();
    var session = await sessionService.CreateAsync(
        sessionOptions,
        new Stripe.RequestOptions { ApiKey = stripeSecretKey },
        cancellationToken);

    return new CreatePaymentMethodSetupSessionResult(session.Url);
}
```

Webhook processing should listen for setup completion and synchronize payment methods.

---

## Listing Saved Payment Methods

Cinturon360 should be able to query payment methods on file for a ProviderCustomer.

Use the seller provider connection.

Example:

```text
HelloWorld Travel Stripe secret key
  -> list payment methods for Coca-Cola's Stripe Customer ID
```

Cinturon360 should display safe card metadata in UI:

```text
American Express ending 8431, expires 03/2044
Mastercard ending 4444, expires 12/2033
```

Cinturon360 stores/syncs provider payment methods locally so travel policies can reference internal IDs.

---

## Travel Policy Payment Rules

A Client organisation can have multiple payment methods.

Example for Coca-Cola:

```text
CCA default payment method
CCA EMEA travel policy payment method
CCA APAC travel policy payment method
CCA NA travel policy payment method
```

Each travel policy has its own booking-time processor. That processor collates information including billing.

If the travel policy billing flag is `Default`, the booking uses the default billing configuration stored on the Client organisation, as configured by the parent TMC.

If the travel policy has a specific payment method, the booking uses that payment method.

Recommended entity:

```csharp
public sealed class TravelPolicyBillingRule
{
    public Guid Id { get; set; }

    public Guid OrganisationId { get; set; }
    public Guid TravelPolicyId { get; set; }

    public BillingResolutionMode ResolutionMode { get; set; }

    // Null when ResolutionMode = Default.
    public Guid? ProviderPaymentMethodId { get; set; }

    public Guid? PaymentProviderConnectionId { get; set; }
    public Guid? ProviderCustomerId { get; set; }

    public BillingMode? BillingModeOverride { get; set; }
    public PaymentCollectionMode? CollectionModeOverride { get; set; }

    public bool IsEnabled { get; set; }

    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
```

```csharp
public enum BillingResolutionMode
{
    Default = 0,
    SpecificPaymentMethod = 1,
    SpecificBillingProfile = 2,
    Manual = 3,
    External = 4,
    NoCharge = 5
}
```

### Travel Policy Examples

```text
CCA Org Default:
  Provider = HwTravel Stripe
  Customer = CCA in HwTravel Stripe
  Payment method = CCA default account fees card
  Billing mode = PeriodicInvoice
  Collection = Automatic
```

```text
CCA APAC Policy:
  ResolutionMode = SpecificPaymentMethod
  Payment method = APAC travel card
  Billing mode = ImmediatePayment
```

```text
CCA EMEA Policy:
  ResolutionMode = SpecificPaymentMethod
  Payment method = EMEA travel card
  Billing mode = PeriodicInvoice
```

```text
CCA NA Policy:
  ResolutionMode = Default
```

Result:

```text
APAC booking -> APAC card
EMEA booking -> EMEA card
NA booking -> CCA default billing setup
```

---

## Booking Billing Resolution

Travel policy processors should not directly call Stripe.

They should resolve a billing instruction.

The billing/payment layer executes the instruction.

Recommended resolver output:

```csharp
public sealed class ResolvedBillingInstruction
{
    public Guid PaymentProviderConnectionId { get; init; }

    public Guid? ProviderCustomerId { get; init; }

    public Guid? ProviderPaymentMethodId { get; init; }

    public BillingMode BillingMode { get; init; }

    public BillingFrequency BillingFrequency { get; init; }

    public PaymentCollectionMode CollectionMode { get; init; }

    public ChargeTreatment ChargeTreatment { get; init; }

    public BillingResolutionSource Source { get; init; }
}
```

```csharp
public enum BillingResolutionSource
{
    OrganisationDefault = 0,
    TravelPolicyOverride = 1,
    BookingOverride = 2
}
```

Example resolver:

```csharp
public sealed class BookingBillingResolver
{
    public ResolvedBillingInstruction Resolve(
        OrganisationBillingProfile orgBilling,
        TravelPolicyBillingRule? policyRule)
    {
        if (policyRule is null || policyRule.ResolutionMode == BillingResolutionMode.Default)
        {
            return new ResolvedBillingInstruction
            {
                PaymentProviderConnectionId = orgBilling.PaymentProviderConnectionId,
                ProviderCustomerId = orgBilling.DefaultProviderCustomerId,
                ProviderPaymentMethodId = orgBilling.DefaultProviderPaymentMethodId,
                BillingMode = orgBilling.DefaultBillingMode,
                BillingFrequency = orgBilling.DefaultBillingFrequency,
                CollectionMode = orgBilling.DefaultCollectionMode,
                ChargeTreatment = orgBilling.DefaultChargeTreatment,
                Source = BillingResolutionSource.OrganisationDefault
            };
        }

        if (policyRule.ResolutionMode == BillingResolutionMode.SpecificPaymentMethod)
        {
            return new ResolvedBillingInstruction
            {
                PaymentProviderConnectionId = policyRule.PaymentProviderConnectionId
                    ?? orgBilling.PaymentProviderConnectionId,

                ProviderCustomerId = policyRule.ProviderCustomerId
                    ?? orgBilling.DefaultProviderCustomerId,

                ProviderPaymentMethodId = policyRule.ProviderPaymentMethodId,

                BillingMode = policyRule.BillingModeOverride
                    ?? orgBilling.DefaultBillingMode,

                BillingFrequency = orgBilling.DefaultBillingFrequency,

                CollectionMode = policyRule.CollectionModeOverride
                    ?? orgBilling.DefaultCollectionMode,

                ChargeTreatment = orgBilling.DefaultChargeTreatment,

                Source = BillingResolutionSource.TravelPolicyOverride
            };
        }

        throw new InvalidOperationException(
            $"Unsupported billing resolution mode: {policyRule.ResolutionMode}.");
    }
}
```

---

## Organisation Billing Profile

A Client org should have default billing settings configured by its seller/parent TMC.

```csharp
public sealed class OrganisationBillingProfile
{
    public Guid OrganisationId { get; set; }

    // Seller organisation, e.g. HelloWorld Travel.
    public Guid SellerOrganisationId { get; set; }

    public Guid PaymentProviderConnectionId { get; set; }

    public Guid? DefaultProviderCustomerId { get; set; }

    public Guid? DefaultProviderPaymentMethodId { get; set; }

    public BillingMode DefaultBillingMode { get; set; }

    public BillingFrequency DefaultBillingFrequency { get; set; }

    public PaymentCollectionMode DefaultCollectionMode { get; set; }

    public ChargeTreatment DefaultChargeTreatment { get; set; }

    public string CurrencyCode { get; set; } = "AUD";

    public bool IsBillingSetupComplete { get; set; }
}
```

---

## Automatic Invoice Payment

A card being "on file" is not enough to automatically pay invoices.

Stripe needs:

1. a saved PaymentMethod attached to the Customer;
2. a selected default payment method for the Customer or invoice;
3. invoices configured for automatic collection.

For Stripe customer-level invoice default:

```text
customer.invoice_settings.default_payment_method = pm_...
```

For invoices:

```text
collection_method = charge_automatically
auto_advance = true
```

For policy-specific invoices, Cinturon360 should use invoice-level `default_payment_method`.

Example:

```text
CCA account fees invoice -> CCA default payment method
CCA APAC travel invoice -> APAC payment method
CCA EMEA travel invoice -> EMEA payment method
```

Stripe payment method priority includes invoice-specific default payment method before the customer default.

### Stripe Customer Default Payment Method Example

```csharp
var customerService = new Stripe.CustomerService();

await customerService.UpdateAsync(
    customerId,
    new Stripe.CustomerUpdateOptions
    {
        InvoiceSettings = new Stripe.CustomerInvoiceSettingsOptions
        {
            DefaultPaymentMethod = paymentMethodId
        }
    },
    new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
    cancellationToken);
```

### Stripe Invoice Automatic Collection Example

```csharp
var invoiceService = new Stripe.InvoiceService();

var invoice = await invoiceService.CreateAsync(
    new Stripe.InvoiceCreateOptions
    {
        Customer = customerId,
        CollectionMethod = "charge_automatically",
        AutoAdvance = true,

        // Optional: policy-specific payment method.
        DefaultPaymentMethod = policyPaymentMethodId,

        Metadata = new Dictionary<string, string>
        {
            ["c360InvoiceId"] = c360InvoiceId.ToString(),
            ["sellerOrganisationId"] = sellerOrgId.ToString(),
            ["buyerOrganisationId"] = buyerOrgId.ToString(),
            ["travelPolicyId"] = travelPolicyId?.ToString() ?? ""
        }
    },
    new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
    cancellationToken);
```

---

## Invoice Model

Cinturon360 should generate its own invoice and invoice lines first.

Stripe invoice IDs are provider references only.

Recommended entity:

```csharp
public sealed class BillingInvoice
{
    public Guid Id { get; set; }

    public string InvoiceNumber { get; set; } = "";

    public Guid SellerOrganisationId { get; set; }
    public Guid BuyerOrganisationId { get; set; }

    public Guid? BillingRelationshipId { get; set; }
    public Guid? ProviderCustomerId { get; set; }
    public Guid? ProviderPaymentMethodId { get; set; }

    public BillingMode BillingMode { get; set; }
    public BillingFrequency BillingFrequency { get; set; }
    public PaymentCollectionMode CollectionMode { get; set; }

    public PaymentProviderType PaymentProviderType { get; set; }

    public string CurrencyCode { get; set; } = "AUD";

    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public InvoiceStatus Status { get; set; }

    public string? ProviderInvoiceId { get; set; }
    public string? ProviderPaymentIntentId { get; set; }
    public string? ProviderHostedInvoiceUrl { get; set; }
    public string? ProviderInvoicePdfUrl { get; set; }

    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }

    public bool AutoCollectPayment { get; set; }
    public bool IsShadowInvoice { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? FinalisedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}
```

```csharp
public enum InvoiceStatus
{
    Draft = 0,
    Finalised = 1,
    Sent = 2,
    PaymentPending = 3,
    Paid = 4,
    PaymentFailed = 5,
    Void = 6,
    Cancelled = 7,
    ExternallyManaged = 8,
    ShadowOnly = 9
}
```

Recommended line item:

```csharp
public sealed class BillingInvoiceLine
{
    public Guid Id { get; set; }

    public Guid BillingInvoiceId { get; set; }

    public string Description { get; set; } = "";

    public BillingLineType Type { get; set; }

    public Guid? BookingId { get; set; }
    public Guid? TravellerUserId { get; set; }
    public Guid? TravelPolicyId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string CurrencyCode { get; set; } = "AUD";

    public string? ProviderInvoiceItemId { get; set; }
}
```

```csharp
public enum BillingLineType
{
    PlatformLicense = 0,
    ActiveUser = 1,
    BranchLocationModule = 2,
    BookingProcessed = 3,
    SupplierFare = 4,
    SupplierTax = 5,
    SupplierBalance = 6,
    Deposit = 7,
    ServiceFee = 8,
    AmendmentFee = 9,
    CancellationFee = 10,
    RefundProcessingFee = 11,
    PolicyExceptionFee = 12,
    CardSurcharge = 13,
    ReportingModule = 14,
    SupportPackage = 15,
    CreditAdjustment = 16,
    DebitAdjustment = 17,
    Tax = 18
}
```

---

## Stripe Invoice Complexity

Stripe invoices can support:

- multiple line items;
- draft invoices;
- automatic collection;
- manual collection;
- due dates/payment terms;
- tax rates/automatic tax;
- discounts;
- negative invoice items;
- credits;
- adjustments;
- metadata;
- memo/footer/description;
- invoice PDFs;
- hosted invoice pages;
- invoice-specific payment method;
- customer default payment method;
- webhooks.

Example Avanōa → HelloWorld monthly invoice:

```text
Invoice: HelloWorld Travel - April 2026

Line items:
- Platform licence - TMC base package
- 42 active users
- 18 branch/location modules
- 1,240 bookings processed
- GDS/NDC transaction usage
- Reporting module
- Support package
- Credit adjustment
- GST
```

Example HelloWorld → Coca-Cola client invoice:

```text
Invoice: Coca-Cola APAC Travel - April 2026

Line items:
- Domestic flight booking - SYD/MEL - traveller ABC
- International flight booking - SYD/SIN - traveller XYZ
- Service fee
- Booking amendment fee
- Refund processing fee
- Policy exception fee
- Card surcharge
- GST/VAT/tax
```

---

## Creating Stripe Invoice Items From C360 Lines

Pseudo-implementation:

```csharp
foreach (var line in c360Invoice.Lines)
{
    var invoiceItemService = new Stripe.InvoiceItemService();

    var invoiceItem = await invoiceItemService.CreateAsync(
        new Stripe.InvoiceItemCreateOptions
        {
            Customer = stripeCustomerId,
            Currency = c360Invoice.CurrencyCode.ToLowerInvariant(),
            Amount = ToMinorUnits(line.TotalAmount),
            Description = line.Description,
            Metadata = new Dictionary<string, string>
            {
                ["c360InvoiceId"] = c360Invoice.Id.ToString(),
                ["c360InvoiceLineId"] = line.Id.ToString(),
                ["lineType"] = line.Type.ToString(),
                ["bookingId"] = line.BookingId?.ToString() ?? "",
                ["travelPolicyId"] = line.TravelPolicyId?.ToString() ?? ""
            }
        },
        new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
        cancellationToken);

    line.ProviderInvoiceItemId = invoiceItem.Id;
}
```

Then create/finalise invoice:

```csharp
var invoiceService = new Stripe.InvoiceService();

var invoice = await invoiceService.CreateAsync(
    new Stripe.InvoiceCreateOptions
    {
        Customer = stripeCustomerId,
        CollectionMethod = c360Invoice.AutoCollectPayment
            ? "charge_automatically"
            : "send_invoice",
        AutoAdvance = c360Invoice.AutoCollectPayment,
        DefaultPaymentMethod = selectedPaymentMethodId,
        DaysUntilDue = c360Invoice.AutoCollectPayment
            ? null
            : c360Invoice.DueDate is null
                ? null
                : Math.Max(0, c360Invoice.DueDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber),
        Metadata = new Dictionary<string, string>
        {
            ["c360InvoiceId"] = c360Invoice.Id.ToString(),
            ["sellerOrganisationId"] = c360Invoice.SellerOrganisationId.ToString(),
            ["buyerOrganisationId"] = c360Invoice.BuyerOrganisationId.ToString()
        }
    },
    new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
    cancellationToken);

c360Invoice.ProviderInvoiceId = invoice.Id;
c360Invoice.ProviderHostedInvoiceUrl = invoice.HostedInvoiceUrl;
c360Invoice.ProviderInvoicePdfUrl = invoice.InvoicePdf;
```

---

## Immediate Booking Payment

For immediate payment at booking time:

```text
Traveller books travel.
Policy approval passes.
C360 prices booking.
C360 resolves billing instruction.
BillingMode = ImmediatePayment.
C360 charges selected payment method using seller provider connection.
Payment succeeds.
C360 completes booking with supplier.
```

For Stripe, create and confirm a PaymentIntent using:

- Customer ID;
- PaymentMethod ID;
- amount;
- currency;
- off_session = true;
- confirm = true.

Example:

```csharp
var paymentIntentService = new Stripe.PaymentIntentService();

var intent = await paymentIntentService.CreateAsync(
    new Stripe.PaymentIntentCreateOptions
    {
        Customer = stripeCustomerId,
        PaymentMethod = stripePaymentMethodId,
        Amount = ToMinorUnits(amount),
        Currency = currencyCode.ToLowerInvariant(),
        Confirm = true,
        OffSession = true,
        Description = $"Travel booking {bookingReference}",
        Metadata = new Dictionary<string, string>
        {
            ["bookingId"] = bookingId.ToString(),
            ["sellerOrganisationId"] = sellerOrgId.ToString(),
            ["buyerOrganisationId"] = buyerOrgId.ToString(),
            ["travelPolicyId"] = travelPolicyId.ToString()
        }
    },
    new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
    cancellationToken);
```

---

## Authorise Only / Capture Later

Authorise-only means the card network/bank authorises and holds funds, but Cinturon360 does not capture the charge yet.

For Stripe, use PaymentIntent manual capture:

```text
capture_method = manual
```

Use this when payment availability should be checked before final supplier commitment.

Travel use case:

```text
Traveller selects flight.
Policy approval passes.
C360 authorises card for $1,240.
Funds are held.
C360 books/tickets with supplier.
Supplier confirms.
C360 captures the authorised payment.
Payment completes.
```

If supplier booking fails:

```text
C360 cancels/releases the authorisation.
No charge is captured.
Traveller/client is not billed.
```

Useful where:

- price can change;
- seats can disappear;
- supplier booking can fail;
- policy approval may be pending;
- ticketing may happen after reservation;
- a short cooling-off period exists.

Not suitable for long-dated monthly billing because card authorisations expire. Stripe uncaptured PaymentIntents are cancelled after a set number of days, commonly 7 days by default.

### Manual Capture PaymentIntent

```csharp
var paymentIntent = await paymentIntentService.CreateAsync(
    new Stripe.PaymentIntentCreateOptions
    {
        Customer = stripeCustomerId,
        PaymentMethod = stripePaymentMethodId,
        Amount = ToMinorUnits(amount),
        Currency = currencyCode.ToLowerInvariant(),
        Confirm = true,
        OffSession = true,
        CaptureMethod = "manual",
        Metadata = new Dictionary<string, string>
        {
            ["bookingId"] = bookingId.ToString(),
            ["paymentMode"] = "AuthoriseOnly"
        }
    },
    new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
    cancellationToken);
```

### Capture Later

```csharp
await paymentIntentService.CaptureAsync(
    paymentIntentId,
    new Stripe.PaymentIntentCaptureOptions
    {
        AmountToCapture = ToMinorUnits(amountToCapture)
    },
    new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey },
    cancellationToken);
```

### Cancel/Release Hold

```csharp
await paymentIntentService.CancelAsync(
    paymentIntentId,
    cancellationToken: cancellationToken,
    requestOptions: new Stripe.RequestOptions { ApiKey = sellerStripeSecretKey });
```

---

## Booking Payment Status

```csharp
public enum BookingPaymentStatus
{
    NotRequired = 0,
    PendingAuthorisation = 1,
    Authorised = 2,
    AuthorisationFailed = 3,
    CapturePending = 4,
    Captured = 5,
    CaptureFailed = 6,
    Released = 7,
    PaymentFailed = 8,
    BilledLater = 9
}
```

---

## Deposits and Scheduled Balances

For deposit scenarios:

```text
Charge deposit + immediate fees now.
Record remaining balance as future payable.
Charge remaining amount later when due.
```

Example:

```text
Travel package total: $5,000
Deposit required now: $1,000
Booking/service fee: $150
Remaining balance: $4,000
Balance due date: 30 days before departure
```

Immediate charge:

```text
$1,150
  $1,000 supplier deposit
  $150 TMC/platform/service fee
```

Future charge:

```text
$4,000 final supplier balance
```

This is better than authorising the full future amount because authorisations expire and are not reliable for long-dated balances.

Recommended model:

```csharp
public sealed class BookingPaymentSchedule
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Guid BuyerOrganisationId { get; set; }
    public Guid SellerOrganisationId { get; set; }

    public string CurrencyCode { get; set; } = "AUD";

    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountOutstanding { get; set; }

    public List<BookingPaymentScheduleItem> Items { get; set; } = [];
}
```

```csharp
public sealed class BookingPaymentScheduleItem
{
    public Guid Id { get; set; }

    public Guid BookingPaymentScheduleId { get; set; }

    public PaymentScheduleItemType Type { get; set; }

    public decimal Amount { get; set; }

    public DateOnly DueDate { get; set; }

    public PaymentTiming Timing { get; set; }

    public PaymentScheduleItemStatus Status { get; set; }

    public Guid? ProviderPaymentMethodId { get; set; }

    public string? ProviderPaymentIntentId { get; set; }
    public string? ProviderInvoiceId { get; set; }
}
```

```csharp
public enum PaymentScheduleItemType
{
    Deposit = 0,
    SupplierBalance = 1,
    BookingFee = 2,
    ServiceFee = 3,
    CardSurcharge = 4,
    Tax = 5,
    Adjustment = 6
}
```

```csharp
public enum PaymentTiming
{
    Immediate = 0,
    DueOnDate = 1,
    EndOfBillingPeriod = 2,
    Manual = 3
}
```

```csharp
public enum PaymentScheduleItemStatus
{
    Pending = 0,
    Due = 1,
    PaymentPending = 2,
    Paid = 3,
    Failed = 4,
    Cancelled = 5,
    Waived = 6,
    ExternallyManaged = 7
}
```

Scheduled billing jobs should find due schedule items and either:

- create PaymentIntent;
- create invoice;
- add to billing period;
- mark manual/external;
- ignore if shadow/no-charge.

---

## Delayed Billing

Delayed billing means the booking can proceed and charges are collected later.

Example:

```text
Traveller books.
Policy passes.
Booking is confirmed.
Charge lines are recorded.
End of period job creates invoice.
Payment is collected automatically/manually/externally.
```

This should not use authorise-only unless capture is expected within the provider's authorisation window.

Delayed billing credit risk belongs to the seller organisation.

---

## Manual Invoice

Manual invoice mode:

- C360 generates invoice;
- invoice is payable;
- no automatic provider charge occurs;
- finance user marks paid manually or imports payment status;
- optional hosted/PDF invoice can be sent;
- provider may be `Manual`.

---

## External Billing

External billing mode:

- C360 calculates charges;
- C360 may generate invoice/ledger/export;
- payment is collected outside C360;
- provider references may be stored;
- payment status can be manually updated or imported;
- useful for MOTO, finance-system billing, legacy payment systems.

---

## No Charge / Internal / Trial / Shadow Billing

This mode should calculate but not charge.

Use cases:

- free pilot;
- internal demo;
- trial;
- migration period;
- commercial exception;
- partner account;
- sales sandbox.

No payable invoice should be issued.

C360 should still be able to show:

- what charges would have been generated;
- shadow invoice;
- shadow usage;
- shadow booking finance;
- shadow licence usage.

---

## Stripe Webhooks

### Requirement

Each seller organisation's Stripe account should have its own Stripe webhook endpoint pointing to Cinturon360.

No Stripe Connect is required.

The seller organisation owns its own Stripe account. Cinturon360 uses that organisation's stored provider credentials to create customers, setup sessions, payment intents, invoices and webhook endpoints.

### Endpoint Pattern

Use one endpoint per provider connection.

Recommended:

```http
POST /api/v1/webhooks/payment-providers/stripe/{connectionId}
```

or:

```http
POST /api/v1/webhooks/stripe/{connectionId}
```

Do not create many endpoints per TMC/use case. One endpoint per provider connection is enough.

### Webhook Secret

Each provider connection must have its own webhook signing secret stored in Key Vault.

DB stores only:

```text
WebhookEndpointId
WebhookSecretReference
```

### Stripe Events To Handle

Initial useful events:

- `checkout.session.completed`;
- `setup_intent.succeeded`;
- `payment_method.attached`;
- `customer.updated`;
- `payment_intent.succeeded`;
- `payment_intent.payment_failed`;
- `invoice.payment_succeeded`;
- `invoice.payment_failed`;
- `invoice.finalized`;
- `charge.refunded`;
- `charge.dispute.created`.

Unhandled events should be logged at debug/information level and return success unless invalid signature.

### Webhook Validation

Webhook endpoint is unauthenticated. It cannot require JWT.

Security comes from Stripe signature verification using the connection-specific webhook secret.

Important:

- read the raw request body;
- do not re-serialize JSON before signature verification;
- lookup `connectionId`;
- load webhook secret from Key Vault;
- call Stripe webhook event construction/verification;
- then route the event.

### Webhook Handler Skeleton

```csharp
[ApiController]
[Route("api/v1/webhooks/payment-providers/stripe")]
public sealed class StripeWebhookController : ControllerBase
{
    [HttpPost("{connectionId:guid}")]
    public async Task<IActionResult> Handle(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var connection = await db.PaymentProviderConnections
            .SingleOrDefaultAsync(x => x.Id == connectionId, cancellationToken);

        if (connection is null || connection.ProviderType != PaymentProviderType.Stripe)
            return NotFound();

        var webhookSecret = await secretStore.GetSecretValueAsync(
            connection.WebhookSecretReference!,
            cancellationToken);

        var json = await new StreamReader(HttpContext.Request.Body)
            .ReadToEndAsync(cancellationToken);

        var signature = Request.Headers["Stripe-Signature"].ToString();

        Stripe.Event stripeEvent;

        try
        {
            stripeEvent = Stripe.EventUtility.ConstructEvent(
                json,
                signature,
                webhookSecret);
        }
        catch
        {
            return BadRequest();
        }

        await stripeWebhookDispatcher.DispatchAsync(
            connectionId,
            stripeEvent,
            cancellationToken);

        return NoContent();
    }
}
```

---

## Stripe Webhook Endpoint Creation During Provider Onboarding

When a TMC/Vendor configures Stripe:

1. create `PaymentProviderConnection` in `PendingSetup`;
2. store Stripe API keys in Key Vault;
3. validate Stripe key;
4. create Stripe webhook endpoint via Stripe API using seller's Stripe key;
5. store returned webhook endpoint ID in DB;
6. store returned webhook signing secret in Key Vault;
7. mark connection `Verified`.

Webhook URL example:

```text
https://api.cinturon360.com/api/v1/webhooks/payment-providers/stripe/{connectionId}
```

Important sequencing:

- connection ID is required before webhook URL can be created;
- webhook signing secret is returned after endpoint creation;
- store webhook secret in Key Vault;
- never store webhook secret raw in DB.

---

## Stripe Provider Gateway Abstraction

Application layer should depend on a provider-neutral abstraction.

```csharp
public interface IPaymentProviderGateway
{
    PaymentProviderType ProviderType { get; }

    Task<CreateProviderCustomerResult> CreateCustomerAsync(
        PaymentProviderContext context,
        CreateProviderCustomerRequest request,
        CancellationToken cancellationToken);

    Task<CreatePaymentMethodSetupResult> CreatePaymentMethodSetupAsync(
        PaymentProviderContext context,
        CreatePaymentMethodSetupRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProviderPaymentMethodSnapshot>> ListPaymentMethodsAsync(
        PaymentProviderContext context,
        string providerCustomerId,
        CancellationToken cancellationToken);

    Task<CreatePaymentResult> CreatePaymentAsync(
        PaymentProviderContext context,
        CreatePaymentRequest request,
        CancellationToken cancellationToken);

    Task<CreateInvoiceResult> CreateInvoiceAsync(
        PaymentProviderContext context,
        CreateInvoiceRequest request,
        CancellationToken cancellationToken);

    Task<RefundPaymentResult> RefundAsync(
        PaymentProviderContext context,
        RefundPaymentRequest request,
        CancellationToken cancellationToken);

    Task<CapturePaymentResult> CaptureAsync(
        PaymentProviderContext context,
        CapturePaymentRequest request,
        CancellationToken cancellationToken);

    Task<CancelPaymentResult> CancelAsync(
        PaymentProviderContext context,
        CancelPaymentRequest request,
        CancellationToken cancellationToken);
}
```

Provider context:

```csharp
public sealed record PaymentProviderContext(
    Guid PaymentProviderConnectionId,
    Guid OwnerOrganisationId,
    PaymentProviderType ProviderType,
    bool IsLiveMode,
    string SecretBundleReference);
```

Payment method snapshot:

```csharp
public sealed record ProviderPaymentMethodSnapshot(
    string ProviderPaymentMethodId,
    string? Brand,
    string? Last4,
    int? ExpiryMonth,
    int? ExpiryYear,
    string? CardholderName,
    string? Fingerprint,
    bool IsReusable);
```

---

## Amount Handling

Cinturon360 should store amounts in major currency units:

```text
49.99 AUD
```

Stripe expects minor units for most currencies:

```text
4999
```

Use a provider/currency-aware converter.

For MVP, simple cents conversion may be acceptable for AUD/USD/EUR/GBP/NZD/CAD.

```csharp
public static long ToMinorUnits(decimal amount)
{
    return (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}
```

Future implementation must handle zero-decimal currencies and provider-specific currency rules.

---

## Idempotency

Idempotency is mandatory before production.

Areas requiring idempotency:

- provider customer creation;
- payment method setup session creation;
- PaymentIntent creation;
- invoice item creation;
- invoice creation;
- webhook processing;
- balance crediting;
- scheduled payment collection.

Recommended local table:

```csharp
public sealed class ProviderIdempotencyRecord
{
    public Guid Id { get; set; }

    public Guid PaymentProviderConnectionId { get; set; }

    public string OperationKey { get; set; } = "";

    public string? ProviderRequestId { get; set; }
    public string? ProviderObjectId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
```

Recommended webhook table:

```csharp
public sealed class ProviderWebhookEvent
{
    public Guid Id { get; set; }

    public Guid PaymentProviderConnectionId { get; set; }

    public PaymentProviderType ProviderType { get; set; }

    public string ProviderEventId { get; set; } = "";

    public string EventType { get; set; } = "";

    public string RawPayloadJson { get; set; } = "";

    public WebhookProcessingStatus Status { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
```

Add unique constraint:

```text
(PaymentProviderConnectionId, ProviderEventId)
```

This prevents duplicate webhook processing.

---

## Current Prepaid Top-Up Flow

The previous Stripe docs described a narrow top-up flow.

That may remain as one billing mode:

```text
PrepaidBalance
```

But it must be implemented using the same provider-neutral billing model.

Prepaid top-up should become:

- seller/buyer aware;
- provider connection aware;
- ProviderCustomer aware;
- ProviderPaymentMethod aware if saved method is used;
- idempotent;
- ledger-backed;
- webhook-backed.

Prepaid top-up must not be the entire Stripe architecture.

---

## Provider-Owned Accounts, Not Stripe Connect

Do not implement Stripe Connect for the TMC model described here.

Each TMC/vendor uses its own Stripe account.

Cinturon360 stores the TMC/vendor Stripe credentials in Key Vault and uses those credentials to operate on behalf of that organisation.

Example:

```text
HelloWorld Travel owns its Stripe account.
Coca-Cola is a Stripe Customer inside HelloWorld's Stripe account.
Money goes to HelloWorld's Stripe account.
Cinturon360 orchestrates customer setup, payment methods, invoices and payments.
```

This is not Stripe Connect.

Stripe Connect would be relevant only if Cinturon360 was acting as a marketplace/platform account and routing/splitting funds to connected accounts.

That is explicitly not the desired model for this design.

---

## Onboarding Flow: Avanōa Bills TMC

Scenario:

```text
Avanōa Technology onboards HelloWorld Travel.
Avanōa chooses Stripe as billing provider.
HelloWorld is billed monthly based on its Org License and usage.
```

Flow:

1. Avanōa has a verified Stripe `PaymentProviderConnection`.
2. Avanōa creates/onboards HelloWorld Travel organisation.
3. C360 creates `BillingRelationship`:
   - SellerOrganisationId = Avanōa;
   - BuyerOrganisationId = HelloWorld;
   - PaymentProviderConnectionId = Avanōa Stripe;
   - BillingMode = PeriodicInvoice;
   - CollectionMode = Automatic;
   - AutoCollectPayment = true;
   - GenerateInvoices = true;
   - CommercialRiskOwner = Platform or SellerOrganisation depending commercial structure.
4. C360 creates `ProviderCustomer` for HelloWorld inside Avanōa's Stripe account.
5. C360 generates billing setup link.
6. HelloWorld finance/admin completes payment setup via Stripe-hosted setup mode.
7. C360 stores `ProviderPaymentMethod`.
8. C360 marks billing setup complete.
9. Monthly job calculates licence/usage.
10. C360 creates internal invoice and lines.
11. C360 creates Stripe invoice/items using Avanōa Stripe credentials.
12. Stripe charges HelloWorld automatically.
13. Stripe webhook updates C360 invoice/payment state.

---

## Onboarding Flow: TMC Bills Client

Scenario:

```text
HelloWorld Travel onboards Coca-Cola.
HelloWorld chooses Stripe as payment provider.
Coca-Cola can be immediate billed, billed later, weekly/monthly billed, or manually/externally billed.
```

Flow:

1. HelloWorld has verified Stripe `PaymentProviderConnection`.
2. HelloWorld creates Coca-Cola Client organisation.
3. C360 creates `BillingRelationship`:
   - SellerOrganisationId = HelloWorld;
   - BuyerOrganisationId = Coca-Cola;
   - PaymentProviderConnectionId = HelloWorld Stripe;
   - BillingMode = configured by HelloWorld;
   - BillingFrequency = configured by HelloWorld;
   - CollectionMode = Automatic/Manual/External;
   - AutoCollectPayment = configured by HelloWorld;
   - CommercialRiskOwner = SellerOrganisation.
4. C360 creates `ProviderCustomer` for Coca-Cola inside HelloWorld's Stripe account.
5. C360 generates setup link.
6. Coca-Cola finance/admin enters payment method directly into Stripe-hosted UI.
7. C360 stores payment method references and safe metadata.
8. Coca-Cola/HwTravel configures default billing and travel policy payment rules.
9. Booking-time resolver decides payment action based on org default or travel policy override.

---

## Booking Flow: Immediate Payment

```text
Traveller from Coca-Cola starts booking.
Traveller has APAC travel policy.
Policy passes approval.
Booking is priced.
Billing resolver finds APAC payment method.
C360 creates immediate PaymentIntent using HelloWorld Stripe key.
Payment succeeds.
C360 completes supplier booking/ticketing.
Payment and booking records are updated.
```

If payment fails:

- booking should not complete if payment-before-ticketing is required;
- booking may move to payment-failed/manual-review state;
- policy/commercial configuration determines retry/manual handling.

---

## Booking Flow: Delayed Billing

```text
Traveller from Coca-Cola starts booking.
Policy passes.
Booking is priced.
BillingMode = PeriodicInvoice.
C360 records BookingFinanceLines.
C360 confirms supplier booking.
End-of-period billing job creates invoice.
Stripe/manual/external collection occurs based on BillingRelationship.
```

Delayed billing risk belongs to HelloWorld Travel.

---

## Booking Flow: Deposit + Future Balance

```text
Booking total = $5,000.
Deposit due now = $1,000.
Immediate service fee = $150.
Future balance = $4,000 due 30 days before departure.
```

C360 creates:

- BookingPaymentSchedule;
- immediate schedule item for deposit;
- immediate schedule item for service fee;
- future schedule item for supplier balance.

Immediate payment is collected now.

Future scheduled job charges/voices later.

---

## Booking Flow: Authorise Only

Use only when capture is expected soon.

```text
Payment is authorised.
Supplier booking is attempted.
If supplier confirms, capture payment.
If supplier fails, cancel/release authorisation.
```

Do not use authorise-only for month-end billing.

---

## ADM / ACM Consideration

Cinturon360 should be designed to support Agency Debit Memo and Agency Credit Memo tracking later.

ADM/ACM events should be tied to:

- booking;
- ticket/EMD/document;
- traveller;
- client org;
- TMC;
- vendor;
- travel policy;
- original invoice/payment;
- refund/reissue/exchange;
- reason code;
- dispute status;
- final settlement.

This matters for travel finance reconciliation and margin leakage control.

Initial MVP does not need full ADM/ACM automation, but invoice/booking finance models should not block it.

---

## GDS / Content Import Consideration

GDS/NDC/supplier content import should feed booking finance.

Data imported from Amadeus, Sabre, Travelport, NDC APIs or supplier feeds should eventually normalize into:

- Booking;
- BookingTraveller;
- BookingSegment;
- BookingTicket;
- BookingFare;
- BookingTax;
- BookingFee;
- BookingPayment;
- BookingRemark;
- BookingDocument;
- BookingFinanceLine.

Billing and invoicing should consume normalized finance lines, not raw GDS payloads.

Store raw provider payloads for audit/reconciliation.

---

## Accounting / Finance Exports

Cinturon360 should support export/integration to external finance systems.

Examples:

- Oracle NetSuite;
- Oracle Financials;
- SAP;
- Microsoft Dynamics 365 Finance;
- Xero;
- MYOB;
- QuickBooks;
- Sage Intacct;
- Workday Financial Management;
- Coupa;
- Concur;
- TechnologyOne;
- CSV/SFTP;
- API-based ledger exports.

Export must use Cinturon360's internal billing ledger/invoice records, not provider-specific Stripe records.

---

## Migration Requirements From Existing Stripe Docs

The existing Stripe docs/runbook should be rewritten to reflect this architecture.

Remove or demote the assumption that Stripe is only for prepaid top-ups.

Update docs to include:

1. Provider-neutral billing architecture.
2. Organisation hierarchy vs billing relationship separation.
3. Seller/buyer terminology.
4. ProviderConnection entity.
5. Azure Key Vault secret storage.
6. ProviderCustomer entity.
7. ProviderPaymentMethod entity.
8. Multiple payment methods per buyer org.
9. Travel policy payment rules.
10. Stripe setup mode/SetupIntent for saving payment methods.
11. Stripe invoice automatic collection rules.
12. Invoice-specific payment methods.
13. Webhook endpoint per provider connection.
14. Idempotent webhook handling.
15. Immediate booking payment.
16. Delayed billing.
17. Deposits and scheduled balances.
18. Authorise-only/capture-later.
19. Manual/external/no-charge/shadow billing.
20. C360 internal ledger as source of truth.
21. Finance export direction.

---

## Implementation Order Recommendation

Recommended order:

1. Add provider-neutral billing entities and enums.
2. Add Azure Key Vault secret store.
3. Add PaymentProviderConnection management.
4. Add Stripe provider connection onboarding.
5. Add ProviderCustomer.
6. Add Stripe payment-method setup session.
7. Add ProviderPaymentMethod sync/listing.
8. Add OrganisationBillingProfile.
9. Add TravelPolicyBillingRule.
10. Add booking billing resolver.
11. Add immediate PaymentIntent charge.
12. Add invoice model and invoice line model.
13. Add Stripe invoice generation.
14. Add webhook endpoint per provider connection.
15. Add webhook idempotency table.
16. Add deposit/scheduled payment model.
17. Add authorise-only/capture-later states.
18. Add manual/external/shadow billing paths.
19. Add reporting/exports.

---

## Source References

These sources were used to verify external provider/platform mechanics:

- Stripe: Save and reuse payment methods / setup mode / SetupIntents  
  https://docs.stripe.com/payments/save-and-reuse

- Stripe: Checkout setup mode  
  https://docs.stripe.com/payments/checkout/save-and-reuse

- Stripe: SetupIntents API  
  https://docs.stripe.com/api/setup_intents

- Stripe: Billing collection methods  
  https://docs.stripe.com/billing/collection-method

- Stripe: Invoices API / invoice object  
  https://docs.stripe.com/api/invoices/object

- Stripe: PaymentIntent capture / uncaptured PaymentIntents  
  https://docs.stripe.com/api/payment_intents/capture

- Stripe: Authorization and capture with PaymentIntents  
  https://support.stripe.com/questions/using-authorization-and-capture-with-paymentintents

- Azure: Key Vault secrets client library for .NET  
  https://learn.microsoft.com/en-us/azure/key-vault/secrets/quick-create-net

- Azure: SecretClient class  
  https://learn.microsoft.com/en-us/dotnet/api/azure.security.keyvault.secrets.secretclient

- Azure: Managed identity with Key Vault for .NET web app  
  https://learn.microsoft.com/en-us/azure/key-vault/general/tutorial-net-create-vault-azure-web-app
