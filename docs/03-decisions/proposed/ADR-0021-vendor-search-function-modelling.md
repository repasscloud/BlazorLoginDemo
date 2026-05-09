# ADR-0021: Vendor Search Function Modelling

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

`notes.txt` (informal session notes) describes a requirement for a "vendor search function" model: a Vendor organisation can enable or disable specific travel booking categories for its downstream TMC and Client organisations. The categories mentioned include:
- Flights
- Hotels
- Car hire
- Rail
- Hotel chains (e.g., Marriott, Hilton direct booking)
- Tour operators
- Insurance

This is a capability entitlement model layered on top of the billing/licensing model: a `LicenseAgreement` defines commercial terms, but separately, a Vendor can toggle which search verticals are available to a given TMC or Client.

The current domain model has no entity or flag for this concept. There is no `VendorSearchFunction`, no `SearchCategory` enum, and no `OrgSearchPermission`. The only related concept is `OrgType` (`Vendor`, `Tmc`, `Client`), which is structural rather than functional.

Provider-side, only Duffel (flights) is integrated. Hotels, cars, rail, and tour operators have empty `Cinturon360.Integrations/{Hotels,Cars,Rail}/` folders with no provider chosen.

Evidence:
- `notes.txt` — search function category model
- `Cinturon360.Integrations/` directory structure
- `src/Cinturon360.Domain/Enums/System/OrgType.cs`
- Absence of any search-function entity or flag in `AppDbContext`

## Decision

*Not yet decided.* The decision must specify:

1. **The data model for search function entitlements:**

   **Option A — Flat flag table on Organisation:**
   ```
   OrgSearchFunction { OrgId, Category (enum), IsEnabled, EnabledBy (VendorOrgId), EnabledAt }
   ```
   Simple; each org has a set of enabled categories. Admin endpoint enables/disables per org per category.

   **Option B — Entitlement within LicenseAgreement:**
   Add `SearchCategory` as a type of `LicenseAgreementEntitlement`. A `LicenseAgreement` between a Vendor and a TMC/Client defines which categories are included.
   Aligned with the billing model; categories can be gated by billing tier.

   **Option C — Vendor-level default + per-org override:**
   A Vendor sets default enabled categories for all its downstream orgs. Individual orgs can be further restricted (but not expanded beyond the vendor default).
   Supports the Flight Centre model where corporate policy restricts certain booking types.

2. **The `SearchCategory` enum values:** Flights, Hotels, CarHire, Rail, HotelChain, TourOperator, Insurance, Other.

3. **Relationship to provider selection:** When a category is enabled for an org, which provider is used? (e.g., Flights → Duffel or Amadeus; Hotels → TBD). This implies a `CategoryProvider` mapping per org or per vendor.

## Consequences

**Recommended decision:** Option C (Vendor-level default + per-org override) because it matches the commercial model — a Vendor licenses a bundle of categories to a TMC, and the TMC may restrict its Clients to a subset.

**If Option C is chosen:**
- New entities: `VendorSearchDefaults { VendorOrgId, Category, IsEnabled }` and `OrgSearchOverride { OrgId, Category, IsEnabled, OverriddenBy }`.
- Admin endpoints: `PUT /api/v1/vendor/search-defaults/{category}` and `PUT /api/v1/org/{id}/search-override/{category}`.
- At booking creation time, check `OrgSearchOverride` (falling back to `VendorSearchDefaults`) to determine if the requested category is enabled.
- The `LicenseAgreementEntitlement` model (ADR-0007) can reference a `SearchCategory` entitlement for billing tier gating.

**Integration dependency:** This decision is most useful once at least one non-flight provider is integrated. Until Hotels, Cars, or Rail are implemented, only the Flights category needs to be operational.

**Required follow-up:**
- Define `SearchCategory` enum in `Cinturon360.Domain.Enums`.
- Author the entities and a migration.
- Build admin UI for Vendor search-function configuration (Vendor dashboard).
- Link this to `LicenseAgreementEntitlement` when ADR-0007 is implemented.
- Revisit provider assignment model when the first non-Duffel integration is scoped.
