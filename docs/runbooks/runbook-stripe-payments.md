# Stripe Payments — Runbook (Provider Connections)

## Overview

This runbook documents operations for seller-owned Stripe provider connections.

Key changes from legacy mode:
- one Stripe account per seller connection;
- one webhook endpoint per connection;
- setup links for buyer org billing setup;
- provider-customer and provider-payment-method synchronization.

Architecture reference:
- `docs/architecture/stripe-payments.md`

## Local setup

1. Configure a Stripe provider connection using API:
  - `POST /api/v1/billing/provider-connections/stripe`
2. Create billing relationship:
  - `POST /api/v1/billing/relationships`
3. Generate setup link (and optionally email it):
  - `POST /api/v1/billing/relationships/{billingRelationshipId}/setup-link`
4. Configure Stripe webhook endpoint to connection path:
  - `POST /api/v1/webhooks/payment-providers/stripe/{connectionId}`

## Stripe dashboard webhook registration

For each provider connection:
1. Create webhook endpoint in Stripe dashboard.
2. Endpoint URL:
  - `https://api.cinturon360.dev/api/v1/webhooks/payment-providers/stripe/{connectionId}`
3. Subscribe to at least:
  - `checkout.session.completed`
  - `setup_intent.succeeded`
  - `payment_method.attached`
  - `payment_intent.succeeded`
  - `payment_intent.payment_failed`

Store the returned signing secret in secret store as:
- `{SecretBundleReference}--webhook-secret`

## API routes used in operations

- create connection: `POST /api/v1/billing/provider-connections/stripe`
- create relationship: `POST /api/v1/billing/relationships`
- onboarding setup link: `POST /api/v1/billing/relationships/{billingRelationshipId}/setup-link`
- sync/list saved methods: `GET /api/v1/billing/provider-customers/{providerCustomerId}/payment-methods`
- relationship prepaid top-up: `POST /api/v1/organisations/{orgId}/billing/relationships/{billingRelationshipId}/prepaid/topup`
- webhook by connection: `POST /api/v1/webhooks/payment-providers/stripe/{connectionId}`
- webhook by org scope: `POST /api/v1/webhooks/stripe/{vendor|tmc|client}/{orgId}`

## Secret management

Production:
- Use Azure Key Vault via managed identity.
- Secrets are never stored raw in the DB.

Development:
- `InMemorySecretStore` is used when `KeyVault:VaultUri` is not configured.
- Optional config fallback: `Secrets:{secretName}`.

## Webhook troubleshooting

### `400 Missing Stripe-Signature header`
- Stripe endpoint misconfiguration or proxy stripping headers.

### `400 Webhook secret is not configured for this connection`
- Missing `{bundleRef}--webhook-secret` secret for the connection.

### `400 Webhook signature verification failed`
- Wrong secret for endpoint.
- Payload was modified before verification.

### `204` with no visible state change
- Event already processed (`ProviderWebhookEvent` idempotency hit), or event type currently ignored.

## Replay behavior and idempotency

- Webhook events are stored in `ProviderWebhookEvent` with unique key:
  - `(PaymentProviderConnectionId, ProviderEventId)`
- Duplicate Stripe deliveries are safe and return no-op `204`.

## Onboarding support procedure (TMC/Client)

When parent org onboards a child org:
1. Create/verify parent Stripe provider connection.
2. Create billing relationship (`Seller=Parent`, `Buyer=Child`).
3. Generate setup link with recipient email.
4. Confirm webhook delivery for setup events.
5. Validate saved payment method appears in `GET /provider-customers/{id}/payment-methods`.

## Known follow-up work

- Travel policy and expense policy processors still need to consume `TravelPolicyBillingRule` at booking execution time.
- License model revision is still pending and documented in root project notes.
- Provider onboarding webhook endpoint auto-creation in Stripe is not yet automated; endpoint is currently created manually in Stripe Dashboard.

---

## Switching from test to live mode

1. Replace `sk_test_...` with `sk_live_...` and `pk_test_...` with `pk_live_...` in secrets.
2. Register a **new** webhook endpoint in live mode (test and live mode have separate webhook endpoints in Stripe).
3. Update `Stripe:WebhookSecret` with the live webhook signing secret.
4. Verify with a small real transaction before going live.

---

## FAQ

**Q: A user reports their top-up succeeded in the card UI but their balance wasn't credited.**  
A: Check the Stripe Dashboard webhook delivery log for `payment_intent.succeeded`. If it shows a failed delivery, replay the event. If it shows successful delivery, search API logs for `ConfirmTopUpCommand` or `payment_intent.succeeded missing orgId` warnings.

**Q: The API is returning 400 on the webhook endpoint.**  
A: Either the `Stripe-Signature` header is missing, or signature verification failed. Confirm `Stripe:WebhookSecret` matches the signing secret shown in the Stripe Dashboard for the registered endpoint.

**Q: The top-up endpoint returns `billing.stripe_customer_failed`.**  
A: `CreateCustomerAsync` failed. Check API logs for the full Stripe error. Common causes: expired SecretKey, Stripe API outage, invalid email format.

**Q: A new development environment has no Stripe credits.**  
A: Remove or blank all three `Stripe:*` config values to activate stub mode. The full top-up flow runs without any real Stripe calls.
