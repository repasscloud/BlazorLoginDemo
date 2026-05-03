# Stripe Payments — Architecture (Provider-Neutral Model)

## Summary

Stripe is now integrated as a provider implementation under a provider-neutral billing architecture.

Core principle:
- Cinturon360 owns billing logic and ledger state.
- Stripe executes payment operations for the selected seller-owned provider connection.

This implementation supports:
- seller/buyer billing relationships separate from org hierarchy;
- seller-owned Stripe accounts (no Stripe Connect marketplace flow);
- provider customer and payment-method references;
- setup-link generation and optional email dispatch during onboarding;
- connection-scoped webhook endpoints with idempotent event persistence.

## Implemented model

The billing domain now includes:
- `PaymentProviderConnection`
- `BillingRelationship`
- `ProviderCustomer`
- `ProviderPaymentMethod`
- `OrganisationBillingProfile`
- `TravelPolicyBillingRule` (foundational entity)
- `ProviderWebhookEvent` (webhook idempotency + audit)

Legacy entities remain for compatibility while migration completes:
- `OrgBillingConfig`
- `OrgLicense`
- `Invoice`
- `Payment`
- `PrepaidBalance`

## Provider connection and secrets

Stripe credentials are stored via `ISecretStore`.

Production path:
- `AzureKeyVaultSecretStore` (`Azure.Security.KeyVault.Secrets`, `Azure.Identity`)

Dev fallback:
- `InMemorySecretStore` with optional `Secrets:{name}` configuration lookup

Secret naming convention:
- `payment-providers--{ownerOrg}--stripe--{mode}--secret-key`
- `payment-providers--{ownerOrg}--stripe--{mode}--publishable-key`
- `payment-providers--{ownerOrg}--stripe--{mode}--webhook-secret`

## Application abstractions

Provider-neutral gateway:
- `IPaymentProviderGateway`

Stripe implementation:
- `StripePaymentProviderGateway`

Supported gateway operations:
- create provider customer
- create setup-session checkout link (`mode=setup`)
- list saved payment methods
- create payments (PaymentIntent)
- parse and verify webhooks

## API surface

Billing management endpoints:
- `POST /api/v1/billing/provider-connections/stripe`
- `POST /api/v1/billing/relationships`
- `POST /api/v1/billing/relationships/{billingRelationshipId}/setup-link`
- `GET /api/v1/billing/provider-customers/{providerCustomerId}/payment-methods`

Relationship-aware prepaid top-up:
- `POST /api/v1/organisations/{orgId}/billing/relationships/{billingRelationshipId}/prepaid/topup`

Webhook endpoints:
- `POST /api/v1/webhooks/payment-providers/stripe/{connectionId}`
- `POST /api/v1/webhooks/stripe/{orgScope}/{orgId}` where `orgScope ∈ {vendor,tmc,client}`

Legacy webhook remains for compatibility:
- `POST /api/v1/webhooks/stripe`

## Onboarding setup-link flow

1. Parent/seller creates `BillingRelationship` for child/buyer.
2. API resolves seller-owned Stripe `PaymentProviderConnection`.
3. API creates or reuses `ProviderCustomer` inside seller Stripe account.
4. API creates Stripe Checkout setup-mode session.
5. API can email a dedicated setup link using `IEmailService`.

This satisfies the requirement to trigger a dedicated setup link for newly onboarded TMC/client organisations.

## Webhook processing model

For connection-scoped webhooks:
1. Read raw body.
2. Resolve connection.
3. Resolve connection-specific webhook secret from secret store.
4. Verify signature with Stripe SDK.
5. Persist `ProviderWebhookEvent` (unique by `connectionId + providerEventId`).
6. Dispatch event behavior.

Handled event behavior currently includes:
- `payment_intent.succeeded` for relationship prepaid top-up confirmation
- setup-related events (`checkout.session.completed`, `setup_intent.succeeded`, `payment_method.attached`) to trigger payment-method sync

## Travel policy and expense policy direction

`TravelPolicyBillingRule` has been added as the policy-level billing anchor for:
- default billing resolution
- specific payment method overrides
- future expense-policy alignment

Current implementation provides the schema and sync foundations; policy processors still need to call the resolver path in a follow-up phase.

## License-driven billing direction

Billing relationships already include `OrgLicenseId` linkage placeholder.

The dedicated license model revision (commercial and access matrix) is intentionally deferred and tracked in project root notes while keeping the data model ready for that migration.
