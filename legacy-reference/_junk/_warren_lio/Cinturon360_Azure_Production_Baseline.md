# Cinturon360 Production Baseline (Azure, Australia East / Sydney)

This document describes a pragmatic **production baseline** for Cinturon360 on Azure using **managed PostgreSQL** and **Azure Container Apps** (no Kubernetes). It is intended for an infrastructure engineer to implement and to generate a cost model in the Azure Pricing Calculator.

---

## Target experience

- End users browse: `https://app.cinturon360.com`
- API clients call: `https://api.cinturon360.com` (authenticated with access keys / auth)
- Behind the scenes:
  - **2x Web** container replicas (web app)
  - **2x API** container replicas (web API)
  - **1x PostgreSQL** managed instance
  - **1x pgAdmin** (or similar DB admin UI) container (internal-only)

---

## High-level architecture

### Core services

- **Azure Container Apps Environment** (Australia East)
- **Container App: `c360-web`**
  - Public ingress
  - Custom domain: `app.cinturon360.com`
  - Min replicas: 2
  - Sticky sessions: enabled (for initial rollout)
  - Azure Files mount for ASP.NET Data Protection keys
- **Container App: `c360-api`**
  - Public ingress
  - Custom domain: `api.cinturon360.com`
  - Min replicas: 2
  - No IP allowlist at launch (auth keys + abuse controls)
- **Container App: `c360-pgadmin`**
  - Internal ingress only
  - Accessible from internal network only (recommended)
- **Azure Database for PostgreSQL Flexible Server** (managed DB)
- **Azure Container Registry (ACR)** for container images (private)

### Optional future additions (not required to launch)
- Azure Cache for Redis (distributed cache/backplane)
- API Management (API key management, rate limiting, analytics)
- Application Gateway WAF / Front Door (edge controls, WAF)

---

## DNS and domain hosting

### Decision
- **Keep registrar at DNSimple** (or current registrar).
- **Host DNS zone in Azure DNS**.

### Steps
1. Create Azure DNS Zone: `cinturon360.com`
2. Azure DNS assigns 4 name servers to the zone.
3. In DNSimple, update domain nameservers to the 4 Azure DNS name servers (delegation).
4. Create DNS records in Azure DNS for:
   - `app.cinturon360.com`
   - `api.cinturon360.com`

> Notes for implementation:
> - Custom domains for Azure Container Apps require specific DNS validation records (TXT) and record types (CNAME/A) depending on the chosen binding method.
> - The exact records are obtained from the Container Apps custom domain binding wizard/CLI at implementation time.

---

## Container images and CI/CD (GitHub Actions)

### Decision
- Use **GitHub Actions** (source code lives in GitHub).
- Use **Azure Container Registry (ACR)** to store images.

### Registry requirements
- ACR should be **private** (requires auth to pull/push).
- Must be **reachable from GitHub Actions**.

### Recommended “get it live” approach (lowest friction)
- Keep ACR **public network access enabled** initially (private registry; not anonymous).
- Authenticate GitHub Actions to Azure using **OIDC** (no long-lived secrets).
- Push images to ACR.
- Container Apps pulls images from ACR.

### Strict “private network only” approach (later hardening)
If ACR public network access is disabled, GitHub-hosted runners may not be able to push/pull without private network access. The typical solution is:
- ACR **Premium** + Private Endpoint
- Self-hosted GitHub runner inside Azure VNet

This is recommended only when hardening requirements justify the operational overhead.

---

## Container Apps platform

### Decision
- Use **Azure Container Apps** (no Kubernetes).

### Container Apps Environment
- Region: **Australia East**
- Create an environment for Cinturon360 production workloads.

### Container App: `c360-web`
Purpose: User-facing site behind `app.cinturon360.com`

**Ingress**
- External/public ingress: **Enabled**
- Target port: whichever the web container listens on (typically 8080/80 depending on image)
- Custom domain: `app.cinturon360.com`
- TLS: managed certificate (preferred) or uploaded certificate

**Scaling**
- Min replicas: **2**
- Max replicas: (baseline) **10** (tune later)
- Scale trigger: HTTP concurrency and/or CPU (baseline config; tune with metrics)

**Sticky sessions**
- Enabled initially (reduces cross-replica issues for stateful browser sessions).
- Keep in **single revision mode** for sticky sessions (recommended at launch).

**Compute sizing (baseline)**
- Per replica:
  - CPU: **0.5 vCPU**
  - Memory: **1.0 GiB**
- Notes:
  - Increase to 1 vCPU / 2 GiB per replica if Blazor Server concurrency or memory pressure shows up early.

**Storage**
- Azure Files mount for ASP.NET Data Protection keys (see “DataProtection keys” below).

**Secrets**
- Environment variables are provided via Container Apps secrets (preferably sourced from Key Vault).

---

### Container App: `c360-api`
Purpose: Public API behind `api.cinturon360.com`

**Ingress**
- External/public ingress: **Enabled**
- Custom domain: `api.cinturon360.com`
- TLS: managed certificate or uploaded

**Scaling**
- Min replicas: **2** (recommended; avoids API being bottleneck / single point)
- Max replicas: (baseline) **10**
- Scale trigger: HTTP concurrency and/or CPU

**Compute sizing (baseline)**
- Per replica:
  - CPU: **0.5 vCPU**
  - Memory: **1.0 GiB**
- Notes:
  - If API performs heavy work (PDF generation, large exports, cryptography), use 1 vCPU / 2 GiB.

**Auth**
- Clients authenticate via private keys / access keys (as designed).
- Abuse handling: block problematic IPs at ingress later as needed.

**Secrets**
- No `.env` file in production images.
- Use Container Apps secrets / Key Vault sourced values.

---

### Container App: `c360-pgadmin` (DB viewer)
Purpose: Operational DB administration (pgAdmin or equivalent)

**Ingress**
- Internal ingress only (recommended)
- Accessible only from internal network (e.g., via VPN / private connectivity).

**Compute sizing (baseline)**
- 0.25 vCPU
- 0.5 GiB RAM
- Scale: 1 replica (min=1, max=1)

**Security**
- Strong auth (pgAdmin login)
- Restrict access via internal networking; avoid exposing publicly.

---

## PostgreSQL (Managed) - Azure Database for PostgreSQL Flexible Server

### Decision
- Use **Azure Database for PostgreSQL Flexible Server** (managed DB).
- Region: **Australia East**
- Backup redundancy: **LRS** baseline
- High Availability: **toggle-able** baseline (cost comparison)

### Baseline DB specification (recommended starting point)
This is intended to be a solid “production baseline” without bank/government-scale provisioning.

**Compute tier**
- **General Purpose**

**vCores**
- **8 vCores**

**Memory**
- As per chosen SKU in Flexible Server (General Purpose).

**Storage**
- **Premium SSD v2**
- **512 GiB**
  - Rationale: crosses the Premium SSD v2 included performance jump, giving more IO headroom early.

**IOPS / Throughput**
- Start at included baseline for the chosen storage size.
- Provision additional IOPS only after measuring real workload.

**High Availability (HA)**
- Baseline cost model should include both:
  1. HA **Off**
  2. HA **On** (zone-redundant)
- HA significantly increases cost; model both to decide production posture.

**Backups**
- **Retention**: 14–35 days (choose policy)
- **Backup redundancy**: LRS baseline

**Networking**
- Prefer private networking (private access) where feasible for production.
- Avoid public DB exposure if possible.

---

## ASP.NET Data Protection keys (required for multi-replica web)

### Problem
Container local filesystem is ephemeral and per-replica. In multi-replica deployments, ASP.NET Data Protection keys must be shared and persisted to avoid random auth/signing failures.

### Decision
Mount **Azure Files** share into the web container.

**Mount settings**
- Storage: Azure Storage Account + Azure Files Share
  - Share: `web-dataprotection`
  - SubPath: `_data` (or similar)
- Mount path: `/home/app/.aspnet/DataProtection-Keys`
- Read/Write: **true**

> If the API also uses ASP.NET Data Protection (e.g., cookie auth, shared ticket formats), mount the same share into the API container too.

---

## Environment variables and secrets (replace `.env`)

### Decision
- Do not use `.env` files in production container images.
- Use:
  - **Key Vault** as the source of truth (preferred), and
  - deploy-time injection into Container Apps **secrets** and environment variables.

### Implementation approach
1. Store all environment values in Key Vault.
2. GitHub Actions deploy pipeline retrieves required values securely and sets Container Apps secrets.
3. Containers read values via environment variables.

> A simpler initial approach is to store secrets directly as Container Apps secrets and migrate to Key Vault later, but Key Vault is preferred for long-term governance and rotation.

---

## Sticky sessions (web)

### Decision
- Enable sticky sessions on `c360-web` at launch to reduce cross-replica issues for browser sessions.
- Keep “single revision mode” at launch for predictable sticky behavior.

### Important note
Sticky sessions do **not** make session state shared. They only attempt to keep a user pinned to the same replica while it exists. For true shared state, use distributed cache/backplane patterns (Redis). Sticky sessions are acceptable as a launch baseline.

---

## Redis (not required at launch)

### Current decision
- Not used today.
- Use sticky sessions + DataProtection keys mount to launch.

### Add Redis later if needed for
- distributed cache
- shared server-side session state
- SignalR backplane (if required)
- cross-replica coordination

Recommended service:
- **Azure Cache for Redis Standard** (baseline), size to be determined once usage is known.

---

## Cost modelling inputs (Azure Pricing Calculator)

Create separate line items for each of the following to produce a realistic baseline.

### PostgreSQL Flexible Server (Australia East)
Model at least two variants:
1. GP **8 vCores**, Premium SSD v2 **512 GiB**, HA **Off**, LRS backups
2. GP **8 vCores**, Premium SSD v2 **512 GiB**, HA **On**, LRS backups

Set backup retention to the intended policy (14–35 days). Include any additional backup storage beyond the included baseline if modelling large backup footprints.

### Azure Container Apps
Model:
- Container Apps Environment (if calculator includes separate costs)
- `c360-web`: 2 min replicas
- `c360-api`: 2 min replicas
- `c360-pgadmin`: 1 replica

Use these baseline per-replica resources for pricing:
- Web: 0.5 vCPU, 1.0 GiB RAM, min=2
- API: 0.5 vCPU, 1.0 GiB RAM, min=2
- pgAdmin: 0.25 vCPU, 0.5 GiB RAM, min=max=1

> If you need a more conservative baseline for production, model at 1 vCPU / 2 GiB per replica for web + api.

### Azure Container Registry (ACR)
Model:
- ACR Standard (baseline) or Premium (if private endpoints planned immediately)
- Include storage estimate for images (e.g., 20–50 GiB to start)

### Azure Storage (Azure Files)
Model:
- Storage account + Azure Files share for DataProtection keys
- Size: small (e.g., 5–20 GiB) unless keys/logs/etc. are stored (keys alone are tiny)

### Key Vault
Model:
- Key Vault instance (secrets operations; typically low cost)
- Include expected secret operations if you want detailed costs.

### Networking
Depending on final implementation, include:
- VNet
- Private endpoints (if DB and/or ACR private access is used)
- Any egress costs (depends on traffic patterns)

---

## Implementation notes / guardrails

1. **Don’t rely on local disk** in containers for any state that must survive restarts or be shared across replicas.
2. **DataProtection keys must be shared** if web is scaled to multiple replicas.
3. Keep pgAdmin internal-only to avoid unnecessary exposure.
4. Start with simple ingress (Container Apps) and add WAF/edge controls later if required.
5. Use GitHub Actions OIDC to avoid storing Azure credentials in GitHub secrets long term.

---

## Summary of baseline specs

### Compute (Container Apps)
- Web: 2 replicas minimum, 0.5 vCPU / 1.0 GiB each, public ingress, sticky sessions ON
- API: 2 replicas minimum, 0.5 vCPU / 1.0 GiB each, public ingress
- pgAdmin: 1 replica, 0.25 vCPU / 0.5 GiB, internal ingress only

### Storage
- Azure Files mount for DataProtection:
  - Share: `web-dataprotection`
  - Path: `/home/app/.aspnet/DataProtection-Keys`
  - RW: true

### Database
- PostgreSQL Flexible Server:
  - Region: Australia East
  - Tier: General Purpose
  - vCores: 8
  - Storage: Premium SSD v2, 512 GiB
  - HA: model Off and On
  - Backups: LRS, 14–35 days retention

### Registry
- ACR private registry, reachable from GitHub Actions (start with public endpoint enabled + OIDC; harden later)

---

## Next actions for infrastructure owner

1. Create Azure DNS zone `cinturon360.com`, delegate from DNSimple.
2. Create ACR, configure GitHub Actions OIDC deployment pipeline.
3. Create Container Apps Environment (Australia East).
4. Deploy `c360-web`, `c360-api`, `c360-pgadmin` container apps with described ingress and scaling.
5. Create Azure Storage Account + Azure Files share and mount into web (and api if needed).
6. Create PostgreSQL Flexible Server with the baseline spec; model HA on/off.
7. Create Key Vault; migrate `.env` settings into secrets; inject into Container Apps.
8. Bind custom domains + certificates for `app.cinturon360.com` and `api.cinturon360.com`.

