# ADR-0007: Adopt License v2 as Canonical Billing Model and Retire Legacy Entities

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

The `AppDbContext` currently exposes three concurrent billing generations introduced across three separate migrations:

| Generation | Migration | Key Entities |
| --- | --- | --- |
| Legacy (Phase 6) | `Phase3To7_CoreSchema` | `OrgLicense`, `OrgBillingConfig`, `Invoice`, `Payment`, `PrepaidBalance` |
| Provider-neutral (2 May 2026) | `AddProviderNeutralBillingArchitecture` | `PaymentProviderConnection`, `BillingRelationship`, `ProviderCustomer`, `ProviderPaymentMethod`, `ProviderWebhookEvent` |
| License v2 + accounting (3 May 2026) | `RefactorBillingModelToLicenseAgreement` | `LicenseAgreement`, `LicenseAgreementEntitlement`, `LicenseCollectionPolicy`, `BillingAccount`, `BillingInvoice`, `PaymentAttempt`, `JournalEntry`, `JournalLine` |

All three are exposed as `DbSet<>` properties. No application command issues invoices or ledger entries against the License v2 model. The legacy entities are unreferenced by any v5 command handler; they exist only in the schema.

`BillingEntities.cs` is 1 088 lines with 22 entity classes — a maintenance liability that signals model drift.

Evidence:

- `src/Cinturon360.Domain/Entities/Billing/BillingEntities.cs:1–1088`
- `src/Cinturon360.Data/Context/AppDbContext.cs:71–93`
- `docs/architecture/cinturon360-v5-billing-licensing-entitlements-accounting-reference.md`
- `docs/architecture/cinturon360-billing-stripe-architecture-reference.md`

## Decision

*Not yet decided.* The recommended decision is:

1. **License v2 (`LicenseAgreement`, `BillingAccount`, provider-neutral `BillingRelationship`/`PaymentProviderConnection`) is the canonical billing model** for all new development from v5.1 forward.
2. **Legacy entities (`OrgLicense`, `OrgBillingConfig`, `Invoice`, `Payment`, `PrepaidBalance`) are retired**: existing rows are converted to License v2 equivalents via a one-shot migration, then the legacy `DbSet` properties and entity classes are removed.
3. The provider-neutral layer (Generation 2) is retained as the Stripe connection abstraction within the License v2 model.

## Consequences

**If this decision is accepted:**

- A data-conversion migration must be authored: each `OrgLicense` row becomes a `LicenseAgreement` + `BillingAccount`; each `Invoice` row becomes a `BillingInvoice`; each `Payment` row becomes a `PaymentAttempt`.
- The migration must be idempotent and reversible (or a rollback migration authored).
- After the conversion migration is verified, a second migration drops the legacy tables and the `DbSet` properties are removed from `AppDbContext`.
- `BillingEntities.cs` should be split into per-aggregate files under `Domain/Entities/Billing/` (e.g., `LicenseAgreement.cs`, `BillingAccount.cs`, `PaymentProviderConnection.cs`).
- All existing commands targeting legacy entities (`OrgLicense`, `Invoice`, etc.) must be removed or redirected.

**Risks:**

- The conversion migration is a destructive schema change; it must be tested against a production-representative data snapshot before applying.
- If any external system (reporting, analytics export) reads legacy tables directly, it must be updated before the tables are dropped.
- No tests currently exist; the conversion must be verified manually or by writing integration tests first.

**Required follow-up:**

- Author ADR for billing execution layer (invoice orchestration, collection policy enforcement).
- Split `BillingEntities.cs` into per-file entities.
- Write conversion migration.
- Write integration test validating round-trip conversion.
- Remove legacy `DbSet` properties and entity classes post-migration.
