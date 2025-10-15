# Acceptable Use Policy (AUP)
**Version:** 1.1 • **Effective Date:** {{EFFECTIVE_DATE}}  
**Supplier:** {{SUPPLIER_LEGAL_NAME}} (ABN {{SUPPLIER_ABN}}) of {{SUPPLIER_ADDRESS}} (“**Supplier**”)  
**Service:** {{SERVICE_NAME}} SaaS platform and related APIs, integrations, and apps (“**Service**”)  
**Applies to:** Supplier, Vendor L1, Vendor L2 (where engaged), Travel Management Companies (TMC), Client entities, and their authorised end‑users.  

> This AUP governs how the Service may be accessed and used. It flows down through the applicable commercial chain. Breach may result in throttling, suspension, or termination. NSW law applies.

---

## 1. Commercial Hierarchy and Roles
**Supported hierarchy paths**  
- Path A: **Supplier → Vendor L1 → TMC → Client**  
- Path B: **Supplier → Vendor L1 → Vendor L2 → TMC → Client**  
- Path C: **Supplier → TMC → Client**  (no Vendor in chain)

**Role definitions**  
- **Supplier.** Operates the platform and infrastructure. Provides L3 platform support to the upstream party in the chain (Vendor L2 where present, else Vendor L1 or TMC). Custodian of security and global service limits.  
- **Vendor L1.** First‑line support to the next party downstream in the chain (TMC or Client where no TMC). Case triage, account assistance, usage coaching, basic configuration checks. No platform changes.  
- **Vendor L2.** Optional layer. Advanced support to TMC. Complex configuration, integration coordination, incident replication, diagnostic packaging. Single escalation point to Supplier L3.  
- **TMC.** Runs travel fulfilment in the Service. Enforces Client travel policy, maintains fare and content accuracy, manages day‑to‑day Client operations. Primary contact for end‑users.  
- **Client.** Owns policy and data for its tenant. Manages user lifecycle and compliance with this AUP. Nominates privacy and security contacts.

**Support and escalation per path**  

| Path | First contact for end‑users | Escalation 1 | Escalation 2 | Escalation 3 |
|---|---|---|---|---|
| A: Supplier → Vendor L1 → TMC → Client | TMC | Vendor L1 | Supplier (via Vendor L1) | N/A |
| B: Supplier → Vendor L1 → Vendor L2 → TMC → Client | TMC | Vendor L2 | Vendor L1 | Supplier (via Vendor L1) |
| C: Supplier → TMC → Client | TMC | Supplier (via TMC) | N/A | N/A |

Notes: End‑users do not contact Supplier. All tickets flow upstream along the chain for that tenant.

---

## 2. Account, Identity, and Access
2.1 Unique accounts. No shared logins except approved service accounts with least privilege.  
2.2 MFA required where provided. No storage of passwords or tokens in plaintext or code.  
2.3 API keys are confidential. Rotate on compromise, employee exit, or every 180 days.  
2.4 RBAC. Assign minimum roles. Quarterly access review by the entity administering the tenant (TMC unless contract states otherwise).  
2.5 Service accounts for automation must be scoped and logged.  
2.6 Do not bypass SSO, session limits, or device checks.

---

## 3. Data Governance and Privacy
3.1 Comply with the Privacy Act 1988 (Cth) and APPs. Ensure lawful basis.  
3.2 Prohibited data without written approval: health records, biometric identifiers, raw payment card PANs, bulk government IDs, private cryptographic keys, or data classified above “Confidential.”  
3.3 Data minimisation. Upload only what is required for workflows.  
3.4 Retention and deletion follow Client policy and product settings. TMC coordinates data subject requests.  
3.5 Exports must be encrypted at rest and in transit. Do not email raw CSVs containing personal information without encryption.  
3.6 Operational telemetry may be collected. Do not tamper with logging.

---

## 4. Security Requirements
4.1 Maintain patched and encrypted admin endpoints.  
4.2 Keep secrets in a managed store. No secrets in tickets, chat, or source control.  
4.3 Use change control for tenant‑visible configuration. Vendor L2 or TMC keeps audit trail.  
4.4 Report suspected security issues to {{SECURITY_CONTACT_EMAIL}} without delay. No public disclosure before coordinated remediation.  
4.5 No penetration tests or scanning without Supplier’s prior written approval via the chain.  
4.6 No client‑side code injection, userscripts, or unapproved browser extensions in production.

---

## 5. Acceptable Use Rules
5.1 Lawful use only. No illegal, infringing, deceptive, or fraudulent activity.  
5.2 No interference with other tenants or the platform.  
5.3 Do not bypass quotas, rate limits, licensing, or metering.  
5.4 Use documented APIs within limits.  
5.5 Content standards: no unlawful, harassing, hateful, or sexually explicit material; no doxxing.  
5.6 No impersonation or misrepresentation.  
5.7 Scraping of HTML is prohibited; use APIs.  
5.8 Batch large jobs and coordinate with the upstream support layer.  
5.9 Respect consent and unsubscribe rules for messaging.  
5.10 Keep resource use within fair‑use thresholds.

---

## 6. Platform Limits and Throttling
Default envelopes unless otherwise contracted:

| Resource | Default Limit | Notes |
|---|---:|---|
| REST API requests | 600/min/tenant | Token bucket; 429 on breach; obey `Retry‑After` |
| GraphQL operations | 300/min/tenant | Complexity weighted |
| Webhooks | 10,000/day/tenant | Exponential backoff and signing |
| Bulk imports | 2 concurrent/tenant | Queue applies |
| Report exports | 20/hour/tenant | Staged for large sets |
| File upload size | 100 MB/file | Larger by pre‑signed only |
| Background tasks | 5 concurrent/tenant | Coordinate heavy usage |

Supplier may adjust limits to preserve stability.

---

## 7. Prohibited Technical Activities
- Port scans, fuzzing, forced browsing.  
- Reverse engineering beyond non‑excludable rights.  
- Session fixation, token replay, cookie tampering.  
- Geo or fraud‑control evasion via anonymisers.  
- Trial or overage grace abuse.  
- Malware or simulated malware in production.  
- Integrity‑check bypass attempts.

---

## 8. Operational Conduct by Role
**Vendor L1** (paths A/B only)  
- First triage from TMC. No direct platform change.  
- Verify identity. No passwords or MFA codes requested.  
- Escalate to Vendor L2 where present, else to Supplier L3 through the agreed channel.

**Vendor L2** (path B only)  
- Reproduce issues. Gather correlation IDs, timestamps, tenant IDs.  
- Validate configuration and scopes.  
- Package diagnostics and escalate to Supplier L3.  
- Coordinate comms to TMC and back to Vendor L1.

**TMC** (all paths)  
- Primary contact for end‑users.  
- Enforce Client travel policy and data accuracy.  
- Secure API credentials and integrations.  
- Operate booking flows within regulatory rules.

**Client** (all paths)  
- Own tenant policy and user lifecycle.  
- Nominate privacy and security contacts.  
- Ensure processors it engages meet equivalent standards.

**Supplier L3** (all paths)  
- Platform engineering and infra support upstream only.  
- No direct end‑user support.  
- May apply mitigations to protect the platform.

---

## 9. Incident Severity and Response
| Sev | Example | First Response | Work to Restore |
|---|---|---|---|
| P1 | Tenant‑wide outage, critical data exposure | 15 min upstream | 4 hours target |
| P2 | Major degradation, key feature impaired | 1 hour | Workaround in 1 business day |
| P3 | Functional fault | 4 business hours | Plan in 3 business days |
| P4 | Minor issue or query | 1 business day | As scheduled |

Tickets follow the escalation path for the tenant’s hierarchy.

---

## 10. Enforcement
10.1 Informal notice and remediation for minor breaches.  
10.2 Throttling or temporary suspension for repeated or material breaches.  
10.3 Immediate suspension for security or legal risk.  
10.4 Termination for persistent or egregious breaches.  
Time under suspension due to breach is excluded from uptime calculations.

---

## 11. Integrations and Third Parties
Use only approved integrations. Keep connectors current. Do not proxy APIs through untrusted middleware with plaintext credentials. Supplier may disable unstable connectors.

---

## 12. Exports, Sanctions, and Industry Rules
Comply with Australian sanctions and export controls. Travel industry data must align with airline, GDS, PCI DSS (if in scope), and TMC accreditation rules.

---

## 13. Notifications
Security: {{SECURITY_CONTACT_EMAIL}} • Abuse: {{ABUSE_REPORT_EMAIL}} • Legal: {{LEGAL_NOTICES_EMAIL}} • Status: {{STATUSPAGE_URL}}

---

## 14. Audit and Cooperation
Supplier may request evidence of compliance. Vendor L2 and TMC maintain runbooks and provide on request. Client cooperates on data subject requests, legal holds, and lawful access.

---

## 15. Changes to Limits and this AUP
Supplier may adjust limits and update this AUP. Material changes are notified via the chain or in‑product. Continued use after the effective date indicates acceptance.

---

## 16. Governing Law and Venue
New South Wales, Australia law governs. Exclusive jurisdiction of NSW courts and the Commonwealth courts of Australia sitting in NSW.

---

## 17. Definitions
- **Client**: The entity that licenses the Service for its end‑users.  
- **End‑user**: A natural person authorised by Client or TMC to use the Service.  
- **Tenant**: Logical partition dedicated to a Client under a chain.  
- **Upstream/Downstream**: Relative position in a hierarchy path for a tenant.  
- **Vendor L1/L2**: Support layers as defined above.  
- **TMC**: Travel Management Company administering operational use of the Service.

---

## Annex A — Examples of Breach
- Shared “admin” login used in production.  
- 10× documented API volume via parallel scraping.  
- Unencrypted CSV with passport numbers emailed externally.  
- Browser plugin injecting unapproved scripts.  
- Load testing during business hours without notice.  
- Expired API keys left active in CI/CD.

## Annex B — Runbooks (Held by Role)
- **Vendor L2:** Import, incident comms, API key rotation, configuration rollback.  
- **TMC:** Fare content validation, booking exception handling, user lifecycle.  
- **Client:** Data retention and legal hold procedures.

## Annex C — Webhook Retries
Initial attempt then exponential backoff for 24 hours. Signed payloads. Keys rotate every 180 days. Failing endpoints may be disabled until remediation is confirmed.

---

### Execution
Use of the Service constitutes acceptance of this AUP.
