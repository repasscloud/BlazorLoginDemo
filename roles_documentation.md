# Role and Authorization Model

This document describes the role structure, tiers, and responsibilities for the Ava Travel Management Platform.

---

## Overview

The platform uses **flat Identity roles** (strings) plus **claims** (`org_id`, `org_type`, `user_category`, `tmc_id`, `client_id`) to enforce tenant scoping.  
Roles are prefixed to make their tier and scope obvious:

- **Sudo** → break-glass, unfettered access
- **Platform.*** → roles for the hosting technology company and master license holder (Tier 1)
- **Tmc.*** → roles for Travel Management Companies (Tier 2)
- **Client.*** → roles for end-customer organizations (Tier 3)

---

## Role Catalog

### SUDO (Global)

| Role | Description |
|------|-------------|
| `Sudo` | Top-level unfettered access. Always passes authorization (bypass). Reserved for break-glass operations. |

### Platform (Tier 1)

| Role | Description |
|------|-------------|
| `Platform.SuperAdmin` | Full control of the platform, create/manage TMCs & Clients, global policies, global billing. |
| `Platform.SuperUser` | Elevated platform-wide access, just below SuperAdmin. |
| `Platform.Admin` | Administrative access for platform staff. |
| `Platform.Support.Admin` | Manage the support center globally. |
| `Platform.Support.Agent` | Operate support cases/tickets. |
| `Platform.Support.Viewer` | Read-only support view. |
| `Platform.Support.Finance` | Handle finance-related support queries. |
| `Platform.UserAdmin` | Manage platform-level users. |
| `Platform.OrgAdmin` | Manage organizations at platform level. |
| `Platform.PolicyAdmin` | Manage global travel policies and templates. |
| `Platform.SecurityAdmin` | Configure security policies (SSO, MFA, DLP). |
| `Platform.IntegrationAdmin` | Manage global integrations (GDS, APIs). |
| `Platform.Finance.Admin` | Full financial control at platform level. |
| `Platform.Finance.Editor` | Edit finance records. |
| `Platform.Finance.Viewer` | View finance records. |
| `Platform.Sales.Rep` | Sales representative role. |
| `Platform.Sales.Manager` | Sales manager role. |
| `Platform.Sales.Admin` | Sales administration role. |
| `Platform.ReportsViewer` | View global reports. |
| `Platform.DataExporter` | Export global datasets. |
| `Platform.Auditor` | Perform audits at platform level. |
| `Platform.ReadOnly` | Read-only access across the platform. |

### TMC (Tier 2)

| Role | Description |
|------|-------------|
| `Tmc.Admin` | Administrative access for the TMC organization. |
| `Tmc.UserAdmin` | Manage users within the TMC and its clients. |
| `Tmc.PolicyAdmin` | Manage travel policies for TMC clients. |
| `Tmc.SecurityAdmin` | Configure TMC-level security policies. |
| `Tmc.IntegrationAdmin` | Configure TMC-specific integrations (back-office, HRIS). |
| `Tmc.Finance.Admin` | Full TMC-level financial control. |
| `Tmc.Finance.Editor` | Edit TMC finance records. |
| `Tmc.Finance.Viewer` | View TMC finance records. |
| `Tmc.BookingsManager` | Manage bookings and itineraries across TMC clients. |
| `Tmc.TravelAgent` | Day-to-day booking and ticketing for clients. |
| `Tmc.ReportsViewer` | View TMC reports. |
| `Tmc.DataExporter` | Export TMC datasets. |
| `Tmc.Auditor` | Audit TMC operations. |
| `Tmc.ReadOnly` | Read-only TMC-level access. |

### Client (Tier 3)

| Role | Description |
|------|-------------|
| `Client.Admin` | Administrative access for the client org. |
| `Client.UserAdmin` | Manage client org users. |
| `Client.PolicyAdmin` | Manage client-level travel policies. |
| `Client.SecurityAdmin` | Configure client-level security policies. |
| `Client.IntegrationAdmin` | Configure client-specific integrations. |
| `Client.Finance.Admin` | Full client-level financial control. |
| `Client.Finance.Editor` | Edit client finance records. |
| `Client.Finance.Viewer` | View client finance records. |
| `Client.Approver.L1` | Approve requests at level 1. |
| `Client.Approver.L2` | Approve requests at level 2. |
| `Client.Approver.L3` | Approve requests at level 3. |
| `Client.ReportsViewer` | View client reports. |
| `Client.DataExporter` | Export client datasets. |
| `Client.Auditor` | Audit client operations. |
| `Client.ReadOnly` | Read-only client-level access. |
| `Client.Requestor` | Create travel requests/bookings. |

---

## Role Assignment by Tier

| Tier | Typical roles | Responsibilities |
|------|---------------|------------------|
| **SUDO** | `Sudo` | Bypass all restrictions, emergency use only. |
| **Platform** | `Platform.SuperAdmin`, `Platform.Support.*`, `Platform.Finance.*`, `Platform.Sales.*`, `Platform.IntegrationAdmin`, etc. | Manage the entire system: create/manage TMCs & Clients, global config, licensing, billing, integrations, support. |
| **TMC** | `Tmc.Admin`, `Tmc.PolicyAdmin`, `Tmc.Finance.*`, `Tmc.BookingsManager`, `Tmc.TravelAgent`, `Tmc.ReportsViewer` | Operate travel services for their client orgs, manage policies, client relationships, and TMC billing. |
| **Client** | `Client.Admin`, `Client.PolicyAdmin`, `Client.Finance.*`, `Client.Approver.L1-3`, `Client.Requestor` | Manage their own org’s users/policies, approve and request travel, view reports, finance data. |

---

## Notes

- Roles are **flat strings** stored in `AspNetRoles` / `AspNetUserRoles`.
- Tier context is enforced with **claims**: `org_id`, `org_type`, `user_category`, `tmc_id`, `client_id`.
- Authorization handlers combine **role** + **claims** to scope access properly.
- Example usage:  
  ```csharp
  [Authorize(Roles = "Sudo,Platform.SuperAdmin")]
  public class GlobalAdminController : Controller { ... }
  ```

---
