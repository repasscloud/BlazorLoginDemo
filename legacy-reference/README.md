# Cinturon360

Cinturon360 (C360) is a cloud-native, multi-tenant travel management
platform built for Travel Management Companies (TMCs).\
It is designed to support modern travel workflows, supplier
integrations, financial controls, and multi-region deployment.

------------------------------------------------------------------------

## Architecture Overview

Cinturon360 follows a layered architecture:

-   **Shared** -- Domain models, aggregates, static reference data,
    contracts.
-   **API** -- Application layer, orchestration, integrations,
    persistence.
-   **Web** -- Blazor Server front-end and ViewModels.
-   **Docs** -- Public documentation site (separate deployment).

The platform is deployed in **fully isolated regional installations**
(e.g., AU, US, EU).\
There is no cross-region data sharing. Each region operates
independently.

------------------------------------------------------------------------

## Deployment Model

Cinturon360 is deployed per-region in isolated Azure environments.

Each deployment represents a complete and independent installation of:

-   Application services
-   Database
-   Logging infrastructure
-   Secrets / Key Vault
-   Storage
-   Networking

No data, logs, or services are shared across regions.

------------------------------------------------------------------------

## Branching Strategy

Cinturon360 uses an environment-aligned branching model.

### Branch Flow

dev → uat → preprod → main

------------------------------------------------------------------------

### dev

-   Internal development branch.
-   Bleeding edge features.
-   Not exposed outside the organization.
-   Used for active feature work and internal validation.

This branch may contain incomplete or experimental functionality.

------------------------------------------------------------------------

### uat

-   Client, integration, and external vendor testing.
-   Stable features promoted from dev.
-   Used for validating integrations (e.g., GDS, NDC, payment
    providers).
-   Mirrors what will eventually reach production.

Once features are validated in UAT, they progress forward without
modification.

------------------------------------------------------------------------

### preprod

-   Internal production validation environment.
-   Mirrors production configuration as closely as possible.
-   Used to validate deployment upgrades prior to release.
-   Receives code from UAT **without changes**.

No feature modifications occur between:

uat → preprod → main

This ensures production integrity.

------------------------------------------------------------------------

### main

-   Represents the current production version.
-   Always reflects what is deployed in all production regions.
-   Treated as the source of truth for PROD.

Any feature that has progressed to `main` will be included in the next
production deployment across all regions.

------------------------------------------------------------------------

## Promotion Rules

1.  Features originate in `dev`.
2.  Once stable, they are promoted to `uat`.
3.  After client/vendor validation, they are promoted to `preprod`.
4.  After internal production validation, they are promoted to `main`.
5.  Deployment to production environments occurs from `main`.

There are no hotfix branches that bypass the flow unless explicitly
required for critical production issues.

------------------------------------------------------------------------

## Production Integrity Principles

-   No direct commits to `main`.
-   No environment-specific feature changes between uat → preprod →
    main.
-   Preprod must mirror production configuration.
-   Production deployments originate only from `main`.

------------------------------------------------------------------------

## Versioning

Production version is defined by the state of the `main` branch at
deployment time.

All regional production environments run the same version unless
explicitly staggered during rollout.

------------------------------------------------------------------------

## Compliance & Security

Cinturon360 is designed to align with:

-   GDPR
-   ISO 27001 controls
-   SOC 2 principles

Security, tenant isolation, structured logging, and auditing are core
design principles.

------------------------------------------------------------------------

## Contributing

1.  Create feature branch from `dev`.
2.  Submit PR into `dev`.
3.  After internal validation, promote through the standard branch flow.
4.  Do not bypass environment stages.

------------------------------------------------------------------------

## Contact

For sales and licensing inquiries, contact the official sales partner.

------------------------------------------------------------------------

© 2026 Cinturon360
