# Credit & Refund Procedure (Aligns to SLA)

**Document ID:** CRP-001  
**Version:** 1.0  
**Jurisdiction:** NSW, Australia  
**Parties:** Supplier = RePass Cloud Pty Ltd ACN [●]; Vendor (L1) = Avanoua Pty Ltd ACN [●]  
**Effective Date:** [Insert Date]  
**Related Documents:** Master Services Agreement (MSA), Commercial Terms Addendum (CTA), Service Level Agreement (SLA), Price Book & Tiering Schedule, Acceptable Use Policy (AUP), Invoicing & Payment Terms Schedule.

This Procedure forms part of the CTA. Capitalised terms have the meanings in the CTA/MSA/SLA. It prescribes when **service credits** are granted under the SLA, how they are calculated, and whether any **refunds** are payable under law or contract.

---

## 1. Scope
1.1 Applies to the SaaS Platform and Supplier-managed Services billed to Vendor L1.  
1.2 Covers **availability credits**, **response/breach credits** if specified in the SLA, and any **regulatory refunds** required by law.  
1.3 Does not cover third‑party services the Supplier does not control, force majeure, or exclusions in the SLA/AUP/MSA.

## 2. Definitions
- **Affected Tenant:** The Vendor L1 tenant or sub-tenant impacted by an SLA breach.  
- **Availability %:** Monthly uptime percentage per tenant, as defined in the SLA.  
- **Credit:** A non‑refundable amount applied against future Supplier invoices to Vendor L1.  
- **Outage Window:** Period of service unavailability as measured by Supplier’s monitoring tooling per the SLA.  
- **Response Target:** Incident acknowledgement and work‑to‑restore targets set in the SLA.  
- **SLA Credit Table:** The percentage credit tiers defined in the SLA based on achieved Availability % (e.g., 5%, 10%, 25%).

## 3. Eligibility
3.1 Vendor L1’s account must be in good standing with no undisputed overdue amounts at time of claim.  
3.2 Vendor L1 must comply with the AUP and not be suspended for cause.  
3.3 SLA exclusions apply, including scheduled maintenance, emergency maintenance with notice, and events outside Supplier control.  
3.4 Credits apply only to **run fees** for the **Affected Tenant(s)** in the **Affected Month** unless the SLA specifies otherwise.

## 4. Credit Triggers
4.1 **Availability Breach.** Monthly Availability % falls below an SLA tier for an Affected Tenant.  
4.2 **Response Target Breach (if in SLA).** P1/P2 targets missed without approved exceptions.  
4.3 **Data Loss (if in SLA).** Where a data‑protection obligation in a schedule defines quantified credits.

## 5. Credit Calculation
5.1 **Base Amount.** The “Monthly Run Fee” for the Affected Tenant for the Affected Month, excluding usage overage, one‑off fees, taxes, and third‑party pass‑throughs unless the SLA states otherwise.  
5.2 **Credit % by Tier.** Use the **SLA Credit Table** for the achieved Availability %. Example:  
- 99.0–99.9% → 5% of Base Amount  
- 98.0–99.0% → 10% of Base Amount  
- <98.0% → 25% of Base Amount  
5.3 **Multiple Breaches.** The highest single applicable credit for the Affected Month applies. Credits do **not** stack.  
5.4 **Portfolio Cap.** Total credits for Vendor L1 in a billing month are capped at **100% of the aggregate Base Amounts** for all Affected Tenants that month.  
5.5 **GST.** Credits are calculated net of GST. GST is adjusted on the credit note per Australian GST rules.

## 6. Claiming Credits
6.1 **Window.** Vendor L1 must submit credit requests **within 30 days** after the end of the Affected Month.  
6.2 **Method.** Email support or raise a ticket via the portal with subject “SLA Credit Request – [Month][Tenant]”.  
6.3 **Required Information.**  
- Tenant name and ID; environment (prod/non‑prod if applicable)  
- Affected Month and UTC windows of observed impact  
- Incident IDs and impact description  
- Evidence if Supplier telemetry is disputed  
6.4 **Supplier Validation.** Supplier validates using authoritative monitoring records. Where Vendor data differs, Parties confer to reconcile.  
6.5 **Decision & Credit Note.** Supplier aims to decide within **10 business days**. Approved credits are issued via **credit note** and automatically applied to the next invoice(s).

## 7. Application and Expiry
7.1 **Application Order.** Credits apply against the next invoice(s) for the same account until exhausted.  
7.2 **No Cash Refund.** Credits are **not** redeemable for cash or transferable, except where a refund is **required by law** or expressly agreed in the CTA.  
7.3 **Expiry.** Credits **expire 12 months** after issue unless the SLA states a different period.  
7.4 **Downstream Billing.** Vendor L1 manages any pass‑through or allocation to its TMCs/Clients. Supplier’s obligation is to Vendor L1 only.

## 8. Refunds
8.1 **Contractual Position.** Fees are non‑refundable except as expressly provided in the CTA/MSA/SLA or **required by law**.  
8.2 **Australian Consumer Law (ACL).** To the extent the ACL applies, statutory remedies are preserved. If a refund is mandated by law, Supplier will issue a refund to Vendor L1 for the qualifying amount.  
8.3 **Processing.** Refunds, where approved, are paid via the original payment method or EFT within **20 business days** of approval.  
8.4 **Interaction with Credits.** If a refund is issued for the same charges, any prior credits applied to those charges are reversed.

## 9. Exclusions
9.1 No credit for issues caused by: Vendor L1 or its users, third‑party providers selected by Vendor, force majeure, denial of service from Vendor’s network, or breaches of the AUP.  
9.2 No credit for **non‑production** environments unless the SLA specifically includes them.  
9.3 No credit where Vendor L1 prevented Supplier from meeting SLA targets (e.g., access not granted).

## 10. Disputes
10.1 **Good‑Faith Review.** If Vendor L1 disagrees with a credit determination, it may escalate under the dispute resolution clause in the MSA.  
10.2 **Payment of Undisputed Sums.** Undisputed charges remain payable on time.  
10.3 **Evidence.** Parties share relevant logs under confidentiality. Supplier provides summary telemetry upon request.

## 11. Records and Audit
11.1 **Retention.** Supplier and Vendor L1 retain billing and incident records for **7 years**.  
11.2 **Audit.** As per CTA/MSA. Any discovered errors result in adjustment on the next invoice or credit note consistent with Section 5.

## 12. Interaction with Other Schedules
12.1 **SLA Controls Credit % and Triggers.** This Procedure references, but does not alter, the SLA Credit Table and definitions.  
12.2 **Price Book Defines Base Amounts.** Base Amount and in‑scope fee components for credit calculation are set by the Price Book & Tiering Schedule.  
12.3 **Invoicing Schedule Controls Application.** Credit notes and tax treatment follow the Invoicing & Payment Terms Schedule.

## 13. Worked Examples (Illustrative)
**Example A — Availability 98.6%**  
- Base Amount: A$10,000 (ex GST) for Tenant X in July.  
- SLA tier for 98.0–99.0% = 10%.  
- Credit = A$10,000 × 10% = **A$1,000** (credit note). GST adjusted accordingly.  

**Example B — Multiple Incidents Same Month**  
- Availability 97.4% (Tier 25%).  
- Two P2 response breaches also occurred, but credits do not stack.  
- Credit = **25%** of Base Amount only.

## 14. Process Flow
1) Incident occurs and is recorded in monitoring.  
2) Month closes; Supplier publishes monthly availability.  
3) Vendor L1 submits credit request (if any) within 30 days.  
4) Supplier validates against telemetry and SLA.  
5) Supplier issues credit note and applies to next invoice.  
6) Dispute path per Section 10 if needed.

## 15. Order of Precedence
If this Procedure conflicts with the SLA, the **SLA prevails** on triggers and percentages. Otherwise the CTA prevails, then this Procedure, then the MSA, except as required by law.

---

**Execution**  
This Procedure is executed as an attachment to the CTA.

Vendor (L1): _Avanoua Pty Ltd_  
Authorised Signatory: __________________  Date: ___________

Supplier: _RePass Cloud Pty Ltd_  
Authorised Signatory: __________________  Date: ___________
