# ADR-0017: Deployment Target — Azure Container Apps + Bicep

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

`QUESTIONS.md Q9` addressed the deployment target. The answer identified Azure Container Apps (ACA) as the target. However:
- `deploy/azure/` is empty — no Bicep, no ARM, no Pulumi.
- `deploy/caddy/` is empty — no TLS reverse-proxy configuration.
- There is no `compose.staging.yaml` or `compose.prod.yaml`.
- No GitHub Actions workflow exists in `.github/workflows/`.

The three runnable hosts — `Cinturon360.Api`, `Cinturon360.Web`, `Cinturon360.Jobs` — each have a Dockerfile (`Dockerfile.api`, `Dockerfile.web`, `Dockerfile.jobs`) and are multi-stage builds targeting `aspnet:10.0` / `mcr.microsoft.com/dotnet/runtime:10.0`.

Azure Container Apps is a serverless container hosting service built on Kubernetes (KEDA). It supports:
- HTTP-triggered scaling (Web, Api)
- KEDA-based event/queue scaling (Jobs)
- Dapr sidecar (not required for v5.1)
- Built-in TLS via managed certificates (eliminates Caddy for public endpoints)
- Revision management for zero-downtime deployments
- Integration with Azure Container Registry, Azure Key Vault, and Azure Database for PostgreSQL Flexible Server

## Decision

*Not yet decided.* The decision must specify:

1. **Azure services in scope for v5.1 deployment:**

   | Service | Purpose |
   |---|---|
   | Azure Container Apps Environment | Hosts Api, Web, Jobs containers |
   | Azure Container Registry | Stores Docker images |
   | Azure Database for PostgreSQL Flexible Server | Primary database (see ADR-0001 + ADR-0009) |
   | Azure Key Vault | JWT secret, Stripe keys, MailerSend key, GitHub PAT |
   | Azure Storage (optional) | Postgres backup storage (see ADR-0018) |
   | Azure Log Analytics Workspace | Receives ACA stdout logs (see ADR-0015) |

2. **IaC tool:** Bicep (recommended, native Azure, no state file, good VS Code support) vs Terraform vs Pulumi.

3. **TLS strategy:** ACA managed certificates (recommended — eliminates the need for Caddy) vs Caddy as a sidecar or front-end container.

4. **CI/CD pipeline:** GitHub Actions workflows for build → test → push to ACR → deploy to ACA.

5. **Environment tiers:** Development (local compose) + Staging (ACA) + Production (ACA). Or Development + Production only.

Recommended decisions:
- **IaC:** Bicep, stored in `deploy/azure/`.
- **TLS:** ACA managed certificates for the Api and Web containers; no Caddy required.
- **Environments:** Staging + Production, each with its own ACA environment and Postgres Flexible Server instance.
- **Key Vault:** `KeyVaultUri` is set in ACA environment variables; the existing `ISecretStore` → `AzureKeyVaultSecretStore` path is used (already implemented in `Cinturon360.Infrastructure/Secrets/`).

## Consequences

**If these decisions are accepted:**
- Author Bicep modules in `deploy/azure/`:
  - `container-apps-environment.bicep`
  - `postgres-flexible-server.bicep`
  - `key-vault.bicep`
  - `container-registry.bicep`
  - `main.bicep` (orchestrates all modules)
- Author GitHub Actions workflows in `.github/workflows/`:
  - `ci.yml` — build, test, lint
  - `cd-staging.yml` — build → push to ACR → deploy to staging ACA
  - `cd-production.yml` — deploy from staging image to production ACA (manual approval gate)
- Remove `deploy/caddy/` placeholder (or populate it only if Caddy is used for a specific purpose like local TLS in dev).
- Set `KeyVault:VaultUri` in ACA environment variables for staging and production.
- Jobs container: ACA Jobs (event-driven) or ACA replica with a single replica (0 replicas when idle is not possible for `IHostedService`-based workers — minimum 1 replica is required).

**Risks:**
- ACA minimum replicas for the Jobs container incurs cost even during idle periods.
- Blazor Server requires sticky sessions (affinity) in ACA; enable `sessionAffinity: "sticky"` on the Web container app.

**Required follow-up:**
- Confirm Azure subscription, tenant ID, and resource group naming conventions.
- Define networking: public ACA environment (recommended for v5.1 simplicity) vs VNet-injected.
- Define SKU tiers for Postgres Flexible Server (Burstable B2s for staging, General Purpose D4s for production).
- Populate `deploy/azure/` and `.github/workflows/`.
