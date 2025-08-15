# Travel Platform Roles & Usage Matrix

> **Legend**:  
> **Scope** = where the role applies. *System* (platform-wide, all orgs) vs *Org* (within a single customer/org/tenant).  
> These roles map to authorization **policies** in `Program.cs`. Assign roles to users; use policies to gate pages/actions.

---

## System-wide roles (platform scope)

| Role | Scope | Summary | Typical Actions / Permissions | Who Uses It | Notes |
|---|---|---|---|---|---|
| **SuperAdmin** | System | Full control across all orgs | Create orgs; impersonate users; manage billing, policies, integrations; view logs; export data | Platform owners | Use sparingly; log all actions & impersonation reasons |
| **SupportAdmin** | System | Elevated support with impersonation | Unlock/reset accounts; impersonate with reason; org settings read/write (safe subset) | Tier‑2 support | No destructive billing/policy changes; strong audit |
| **SupportFinance** | System | Finance support read + refunds | View invoices/payouts; process refunds within threshold; reconcile | Finance support | Read-mostly; no user or policy admin |
| **SupportAgent** | System | Operative traveler support | View trips, bookings, profiles; assist changes/cancellations | Tier‑1 support | Read-mostly; no finance/policy |
| **SupportViewer** | System | Read-only diagnostics | View logs, health, org info | Observers, auditors | No PII export if possible |

---

## Organization roles (per customer/org)

| Role | Scope | Summary | Typical Actions / Permissions | Who Uses It | Notes |
|---|---|---|---|---|---|
| **OrgAdmin** | Org | Owner of the org | Everything within their org: users, roles, policies, finance, integrations | Org owner / IT lead | Tenant boundary applies |
| **UserAdmin** | Org | User lifecycle admin | Invite users; enable/disable; reset MFA; assign roles | IT helpdesk | No policy/finance edits |
| **PolicyAdmin** | Org | Travel policy designer | Create/edit policies, approval rules, vendor allow/deny lists | Travel program mgr | Changes affect approvals & suppliers |
| **FinanceAdmin** | Org | Full finance control | Payment methods, GL codes, tax, refunds, exports, invoices | Finance controller | High risk; require approvals for destructive ops |
| **FinanceEditor** | Org | Day-to-day finance edits | Update cost centers/POs; issue refunds ≤ threshold | Finance team | Combine with claim `refund_limit` |
| **FinanceViewer** | Org | Finance read-only | View invoices, exports, reports | Finance analysts | No write actions |
| **SecurityAdmin** | Org | Security posture | Configure SSO, MFA enforcement, password policy | Security admin | Usually few users |
| **IntegrationAdmin** | Org | Integrations | API keys, webhooks, HRIS/ERP connectors | Integration engineer | Rotate keys; audit downloads |
| **BookingsManager** | Org | Manage bookings org-wide | Reassign trips; force cancel/change; handle disruptions | Travel desk lead | Be careful with cancellations |
| **TravelAgent** | Org | Book for others | Create/modify bookings for travelers | Executive assistants, travel desk | Often limited to assigned users/teams |
| **ApproverL1** | Org | Approval tier 1 | Approve/deny standard requests per policy | Line managers | Use rule-based routing |
| **ApproverL2** | Org | Approval tier 2 | Approve high-cost or exception trips | Dept heads | Combine with limits |
| **ApproverL3** | Org | Approval tier 3 | Approve exceptional/exec travel | Executives | Rarely used |
| **ReportsViewer** | Org | Analytics access | Run dashboards; view KPIs | Managers, finance | No raw export by default |
| **DataExporter** | Org | Data export rights | Export CSVs; schedule reports | BI/Finance ops | PII handling—log downloads |
| **Auditor** | Org | Read-only across org | View all records + audit logs | Compliance, audit | No PII export ideally |
| **Requestor** | Org | Standard end-user | Request/book travel for self per policy | All employees | Default role on invite |
| **ReadOnly** | Org | Minimal access | View own profile/itinerary only | Contractors | No booking capability |

---

## Suggested policies (mapping)

> Examples you can register in `AddAuthorization` and apply via `@attribute [Authorize(Policy = "...")]`:

- **CanManageUsers** → `SuperAdmin`, `OrgAdmin`, `UserAdmin`  
- **CanEditPolicies** → `SuperAdmin`, `OrgAdmin`, `PolicyAdmin`  
- **CanEditFinancials** → `SuperAdmin`, `OrgAdmin`, `FinanceAdmin`, `FinanceEditor`  
- **FinanceRead** → `SuperAdmin`, `OrgAdmin`, `FinanceAdmin`, `FinanceEditor`, `FinanceViewer`, `SupportFinance`  
- **CanEnableDisableUser** → `SuperAdmin`, `OrgAdmin`, `UserAdmin`  
- **SupportArea** → `SupportViewer`, `SupportAgent`, `SupportFinance`, `SupportAdmin`  
- **ApproverL1OrAbove** → `ApproverL1`, `ApproverL2`, `ApproverL3`, `OrgAdmin`, `SuperAdmin`  
- **ApproverL2OrAbove** → `ApproverL2`, `ApproverL3`, `OrgAdmin`, `SuperAdmin`  
- **ApproverL3OrAbove** → `ApproverL3`, `OrgAdmin`, `SuperAdmin`  

---

## Tips

- Use **claims** for numeric thresholds (e.g., `refund_limit=500`) instead of creating many roles.  
- Keep **Org scope** enforced via a claim like `org_id`, checked by custom handlers or repository filters.  
- Audit: role changes, impersonation, refunds, policy edits, exports.  
- For UI, bundle common roles (e.g., *Finance Team* → `FinanceViewer` + `ReportsViewer`; *Org Owner* → `OrgAdmin` + `PolicyAdmin` + `FinanceAdmin` + `UserAdmin`).

