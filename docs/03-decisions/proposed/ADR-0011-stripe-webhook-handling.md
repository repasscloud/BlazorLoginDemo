# ADR-0011: Stripe Webhook Handling — Idempotency, Signature, and Replay Strategy

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

Cinturon360 processes Stripe webhooks to reconcile payment events with internal billing state. Two webhook routes are currently implemented:

- `POST /api/v1/webhooks/payment-providers/stripe/{connectionId}` — connection-scoped, used by the provider-neutral billing model
- `POST /api/v1/webhooks/stripe/{vendor|tmc|client}/{orgId}` — legacy org-scoped route
- A legacy single-endpoint route also exists

The connection-scoped handler has Stripe signature verification (`StripePaymentGateway` / `StripePaymentProviderGateway`). Idempotency is handled via a unique `(connectionId, providerEventId)` index on `ProviderWebhookEvent`.

Documented gaps (per `BACKLOG.md` and `docs/runbooks/runbook-stripe-payments.md`):

- No idempotency keys on Stripe payment intent creation (Stripe-side duplicate prevention)
- No replay/retry mechanism for failed webhook processing (if the handler throws, Stripe retries — but there is no DLQ or alerting)
- No rotation runbook for `WebhookSecret` per connection
- The org-scoped legacy routes do not have the same idempotency guard as the connection-scoped route
- `docs/architecture/stripe-payments.md` notes that "Legacy entities remain for compatibility while migration completes"

Evidence:

- `src/Cinturon360.Infrastructure/Payments/StripePaymentProviderGateway.cs`
- `src/Cinturon360.Infrastructure/Payments/StripePaymentGateway.cs`
- `docs/architecture/stripe-payments.md`
- `docs/runbooks/runbook-stripe-payments.md`
- `src/Cinturon360.Domain/Entities/Billing/BillingEntities.cs` — `ProviderWebhookEvent`

## Decision

*Not yet decided.* The decision must specify:

1. **Canonical webhook route:** Which route is the long-term canonical endpoint — connection-scoped or org-scoped? (Recommended: connection-scoped only; deprecate org-scoped legacy routes once License v2 is canonical per ADR-0007.)

2. **Signature verification:** All active webhook routes must verify the Stripe webhook signature before processing. The connection-scoped route already does this. The legacy org-scoped routes must be audited.

3. **Idempotency strategy:**
   - Inbound: The `ProviderWebhookEvent` unique index provides event-level deduplication. Extend to the legacy routes or deprecate them.
   - Outbound (payment intent creation): Stripe idempotency keys must be applied using a deterministic key (e.g., `LicenseAgreementId + BillingCycleId`).

4. **Replay strategy:** When webhook processing fails (handler throws after persisting `ProviderWebhookEvent`), define the retry model:
   - **Option A:** Rely on Stripe's built-in retry (Stripe retries up to 3 days). The handler must be idempotent enough to handle re-delivery.
   - **Option B:** A background job re-processes `ProviderWebhookEvent` rows in `Pending` state on a schedule.

5. **Secret rotation policy:** `WebhookSecret` per `PaymentProviderConnection` must be rotatable without downtime. Define the rotation procedure.

## Consequences

**Recommended decisions:**

- Canonical route: connection-scoped only.
- Signature verification: required on all routes; enforced by a middleware or base handler class.
- Idempotency (inbound): `ProviderWebhookEvent` unique index (already implemented); extend status field (`Pending`, `Processed`, `Failed`).
- Idempotency (outbound): use `$"{licenseAgreementId}:{billingCycleId}:{event}"` as the Stripe idempotency key.
- Replay: Option A (rely on Stripe retry) with an alerting job that pages if any `ProviderWebhookEvent` remains in `Pending` state for > 1 hour.
- Secret rotation: documented two-phase rotation (register new secret, drain old, remove old) per `runbook-stripe-payments.md`.

**Required follow-up:**

- Add `Status` field to `ProviderWebhookEvent` (Pending / Processed / Failed).
- Add Stripe idempotency keys to all payment-intent creation calls.
- Deprecate legacy org-scoped webhook routes (set a removal milestone).
- Extend `runbook-stripe-payments.md` with the secret rotation procedure.
- Add an alert job for stale `Pending` webhook events.
