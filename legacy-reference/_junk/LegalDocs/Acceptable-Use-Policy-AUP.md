# Acceptable Use Policy (AUP)
**Version:** 1.0 • **Effective Date:** {{EFFECTIVE_DATE}}  
**Supplier:** {{SUPPLIER_LEGAL_NAME}} (ABN {{SUPPLIER_ABN}}) of {{SUPPLIER_ADDRESS}} (“**Supplier**”)  
**Service:** {{SERVICE_NAME}} SaaS platform and related APIs, integrations, and apps (“**Service**”)  
**Applies to:** Vendor L1, Vendor L2, Travel Management Companies (TMC), Client entities, and their authorised end‑users.  

> This AUP governs how the Service may be accessed and used. It flows down from Supplier’s Master Services Agreement (MSA), Service Level Agreement (SLA), and End‑User Terms/EULA. Breach may result in throttling, suspension, or termination. This AUP is governed by the laws of New South Wales, Australia.

---

## 1. Roles and Responsibilities
**Vendor L1.** First‑line support to Clients and end‑users. Case triage, basic troubleshooting, account assistance, usage coaching.  
**Vendor L2.** Advanced support to Clients and TMC. Configuration, integration coordination, incident replication, logs packaging. Escalates to Supplier L3.  
**TMC.** Operates travel fulfilment workflows, validates content, adheres to data accuracy standards, and enforces Client policy within the Service.  
**Client.** Owns policy, data stewardship, user lifecycle management, and ensures end‑user compliance with this AUP.  
**Supplier L3.** Platform engineering and infrastructure support to Vendor. Not a replacement for Vendor L1/L2.

---

## 2. Account, Identity, and Access
2.1 Unique accounts required. No shared logins except approved service accounts scoped to least privilege.  
2.2 Strong authentication. MFA required where provided. Passwords or tokens must not be stored in plain text or hard‑coded.  
2.3 API keys are confidential. Rotate on compromise, employee exit, or at least every 180 days.  
2.4 Role‑based access control (RBAC). Assign minimum roles. Audit roles quarterly.  
2.5 Automated users and bots must be registered as service accounts with explicit scopes.  
2.6 You must not circumvent SSO, session controls, or device posture checks.

---

## 3. Data Governance and Privacy
3.1 Lawful basis. You must have a lawful basis to process personal information and must comply with the Privacy Act 1988 (Cth) and Australian Privacy Principles.  
3.2 Prohibited data. No special/sensitive categories unless explicitly contracted: health records, biometrics for identification, payment card PANs beyond tokenised forms, government IDs in bulk, secrets (private keys), or data classified above “Confidential” without written approval.  
3.3 Data locality. Data may be stored in regions disclosed in the Privacy Notice. Client must not upload data that would trigger conflicting localisation mandates without Supplier’s prior approval.  
3.4 Data minimisation. Upload only what is required for the relevant workflow. Remove legacy exports.  
3.5 Retention and deletion. Follow Client policy and in‑product retention settings. Vendor L2 ensures deletion requests are logged and actioned.  
3.6 Export safeguards. Exports must be encrypted at rest and in transit. Do not email raw CSVs containing personal information without encryption.  
3.7 Telemetry. Operational logs and metrics may be collected for security, billing, and improvement. Do not attempt to suppress or tamper with logging.

---

## 4. Security Requirements
4.1 Endpoint hygiene. Maintain patched operating systems, anti‑malware, and disk encryption for admin endpoints.  
4.2 Secrets. Use managed secret stores. No secrets in source control, tickets, or chat.  
4.3 Change control. Configuration changes must be tracked and reversible. Vendor L2 uses change tickets for Client‑visible changes.  
4.4 Vulnerability reporting. Report potential security issues to {{SECURITY_CONTACT_EMAIL}} without delay. Do not publicly disclose prior to coordinated remediation.  
4.5 Penetration testing. No testing, scanning, or attack simulation without Supplier’s prior written approval via Vendor. Client‑initiated “response time tests” that simulate attack patterns are prohibited unless authorised.  
4.6 Third‑party code. Browser extensions, userscripts, or client‑side injection that alters Service behaviour are prohibited in production environments.

---

## 5. Acceptable Use Rules
5.1 Lawful use only. No illegal, infringing, deceptive, or fraudulent activity.  
5.2 No interference. Do not overload, disrupt, or degrade the Service, networks, or other tenants.  
5.3 No circumvention. Do not bypass quotas, rate limits, licensing, or metering.  
5.4 No abuse of APIs. Calls must be well‑formed, authenticated, and within documented limits.  
5.5 Content standards. No unlawful, harassing, hateful, or sexually explicit material. No doxxing or stalking.  
5.6 Impersonation. Do not misrepresent identity or affiliation.  
5.7 Scraping. Automated extraction permitted only via documented APIs and within rate limits. HTML scraping of the app is prohibited unless approved in writing.  
5.8 Bulk actions. Use batch or asynchronous endpoints where available. Coordinate large jobs with Vendor L2 to avoid impact.  
5.9 Email and messaging. No spam. Respect unsubscribe, frequency caps, and consent requirements.  
5.10 Fair usage. Use shared resources within thresholds to preserve stability and fairness.

---

## 6. Platform Resource and Rate Limits
Supplier may update limits to preserve stability. Default envelopes unless otherwise contracted:

| Resource | Default Limit | Notes |
|---|---:|---|
| REST API requests | 600 per minute per tenant | Burstable with token bucket; sustained excess may throttle |
| GraphQL operations | 300 ops/min per tenant | Complexity scoring applies |
| Webhooks delivery | 10,000/day per tenant | Retries with exponential backoff |
| Bulk import jobs | 2 concurrent per tenant | Queue additional jobs |
| Report exports | 20 per hour per tenant | Large exports may be staged |
| File upload size | 100 MB per file | Larger by S3 pre‑signed only |
| Background tasks | 5 concurrent per tenant | Coordinate heavy runs with Vendor L2 |

**Throttling policy.** On breach, 429 is returned. Clients must respect `Retry‑After`. Repeated breaches may result in temporary blocks.

---

## 7. Prohibited Technical Activities
- Scanning ports, fuzzing, or forced browsing.  
- Reverse engineering, decompilation, or extracting source except as allowed by law.  
- Session fixation, token reuse, or cookie manipulation.  
- Use of anonymisation relays to evade geo or fraud controls.  
- Abuse of trial, promo, or overage grace bands.  
- Upload of malware, droppers, or test files that mimic malware in production.  
- Tampering with client‑side code integrity checks.

---

## 8. Operational Conduct by Role
**Vendor L1**  
- Verify user identity before account actions.  
- Provide first response within business‑hours targets.  
- Do not request passwords or MFA codes.  
- Collect minimal logs and escalate to Vendor L2 if unresolved in 30 minutes for P2/P1.  

**Vendor L2**  
- Reproduce issues, gather correlation IDs, timestamps, request IDs, and affected tenant IDs.  
- Validate configuration deltas and access scopes before changes.  
- Package diagnostics for Supplier L3 using secure channels.  
- Own communications with Client and TMC during incidents.  

**TMC**  
- Keep policy and content accurate. No manual PII entry into free‑text fields without necessity.  
- Use approved integrations and maintain API credentials securely.  
- Ensure booking flows and fares comply with Client policy and regional regulations.  

**Client**  
- Maintain user lifecycle (joiners, movers, leavers). Disable access promptly on exit.  
- Define data retention and legal hold requirements.  
- Nominate an Information Security contact and a Privacy contact.  
- Ensure third‑party processors engaged by Client meet equivalent standards.

---

## 9. Incident Severity and Response
Severity is assigned by impact to production service and users.

| Severity | Example | First Response | Work to Restore |
|---|---|---|---|
| P1 | Tenant‑wide outage, critical data exposure | 15 min (Vendor L2 to Supplier L3) | Target 4 hours |
| P2 | Major degradation, critical feature impaired | 1 hour | Workaround within 1 business day |
| P3 | Functional fault without major impact | 4 business hours | Action plan within 3 business days |
| P4 | Minor issue or query | 1 business day | As scheduled |

All parties must cooperate, provide logs, and avoid change activity that risks expansion of impact without approval.

---

## 10. Enforcement Process
10.1 Informal notice for minor breaches with remediation steps and timeline.  
10.2 Throttling or temporary suspension for repeated or material breaches.  
10.3 Immediate suspension for security, legal risk, or harm to other tenants.  
10.4 Termination where breaches persist or are egregious.  
10.5 Supplier may block IPs, tokens, or integrations linked to abuse.  

**Effect on SLA.** Time under suspension due to your breach is excluded from uptime calculations. Credits are unavailable for breach‑induced disruption.

---

## 11. Integrations and Third Parties
11.1 Only use integrations listed in documentation or approved in writing.  
11.2 Maintain updated connectors and rotate credentials.  
11.3 Do not proxy Supplier APIs through untrusted middleware that stores plaintext credentials.  
11.4 If a third‑party causes instability, Supplier may disable the connector pending remediation.

---

## 12. Exports, Sanctions, and Industry Rules
12.1 Comply with Australian sanctions laws and relevant export controls. Do not provide access to embargoed regions or denied parties.  
12.2 Travel industry data must be used in accordance with applicable airline, GDS, PCI DSS (if in scope), and TMC accreditation obligations.

---

## 13. Communications and Notifications
13.1 Security incidents: {{SECURITY_CONTACT_EMAIL}}.  
13.2 Abuse reports: {{ABUSE_REPORT_EMAIL}} with timestamps, request IDs, and evidence.  
13.3 Legal notices: {{LEGAL_NOTICES_EMAIL}}.  
13.4 Status page: {{STATUSPAGE_URL}} for maintenance windows and advisories.

---

## 14. Audit and Cooperation
14.1 Supplier may request evidence of compliance with this AUP.  
14.2 Vendor L2 and TMC must maintain runbooks for critical workflows and supply them upon request.  
14.3 Client will cooperate on data subject requests, legal holds, and lawful access requests.

---

## 15. Changes to Limits and this AUP
Supplier may adjust limits and update this AUP to protect the Service or comply with law. Material changes will be notified via Vendor or in‑product notices. Continued use indicates acceptance after the effective date.

---

## 16. Governing Law and Venue
This AUP is governed by the laws of New South Wales, Australia. The parties submit to the exclusive jurisdiction of the courts of New South Wales and the Commonwealth courts of Australia sitting in New South Wales.

---

## 17. Definitions
- **Client**: The entity that licenses the Service from Vendor for its end‑users.  
- **End‑user**: A natural person authorised by Client or TMC to use the Service.  
- **Personal information**: Information about an identifiable individual within the meaning of the Privacy Act 1988 (Cth).  
- **Supplier L3**: Supplier’s platform support engineers.  
- **Tenant**: A logical partition of the Service dedicated to a Client under a Vendor.  
- **Token bucket**: A rate‑limit mechanism allowing short bursts above steady‑state throughput.

---

## Annex A — Examples of Breach
- Using a shared “admin” account for daily work.  
- Pushing 10× the permitted API volume by parallelising scraping.  
- Emailing unencrypted exports with passport numbers.  
- Installing a browser plugin that injects unapproved scripts into the app.  
- Running load tests during business hours without notice.  
- Persisting expired API keys in CI/CD variables.

## Annex B — Operational Runbooks (Vendor L2 / TMC)
- **Bulk Import Runbook.** Pre‑checks, staging validation, job size thresholds, rollback.  
- **API Key Rotation Runbook.** Inventory, dual‑key deployment, cutover, revoke.  
- **Incident Comms Template.** Audience, frequency, channels, record of decisions.  
- **Data Deletion Runbook.** Identity validation, scope, irreversible confirmation, evidence record.

## Annex C — Default Webhook Retries
- Initial attempt, then retries with exponential backoff for up to 24 hours.  
- Signed payloads with key rotation every 180 days.  
- Failing endpoints may be disabled until Vendor L2 confirms remediation.

---

### Execution
Use of the Service constitutes acceptance of this AUP.
