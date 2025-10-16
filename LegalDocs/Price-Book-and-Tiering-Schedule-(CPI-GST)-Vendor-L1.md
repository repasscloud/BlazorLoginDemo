# Price Book & Tiering Schedule (CPI/GST)


**Document ID:** PBTS-001  
**Version:** 1.0  
**Jurisdiction:** NSW, Australia  
**Parties:** Supplier = RePass Cloud Pty Ltd ACN [●]; Vendor (L1) = Avanoua Pty Ltd ACN [●]  
**Effective Date:** [Insert Date]  
**Related Documents:** Commercial Terms Addendum (CTA), Master Services Agreement (MSA), SLA, AUP.

This Price Book & Tiering Schedule (Schedule) forms part of the CTA and applies to the Vendor at Level 1 (L1). Unless defined here, capitalised terms take the meaning in the CTA/MSA.


## 1. Scope
This Schedule sets out list prices, tiered pricing, volume and overage rules, CPI indexation, GST treatment, rounding, and change mechanics for the SaaS Platform and related Services provided by the Supplier to the Vendor L1.


## 2. Definitions
- **ABS CPI (Sydney):** Australian Bureau of Statistics Consumer Price Index, All Groups, Sydney.  
- **Base Price:** The pre-indexation, pre-GST price for a Unit in the Base Period.  
- **Base Period:** The 12‑month period commencing on the Effective Date, or as specified in the CTA.  
- **CPI Adjustment Date:** 1 July each year, unless the Parties agree a different date in the CTA.  
- **CPI%:** The percentage change between the most recently published ABS CPI (Sydney) for the March quarter and the corresponding CPI for the prior year.  
- **GST:** Goods and Services Tax under A New Tax System (Goods and Services Tax) Act 1999 (Cth), currently 10%.  
- **Tier:** A usage or seats band with defined Unit Prices.  
- **Unit:** The measurable billing metric for a SKU (for example: per active user per month, per API call, per tenant, per environment, per 1,000 transactions).


## 3. Price Book (Vendor L1)
Prices are **exclusive of GST** and **subject to CPI indexation** per Section 5. Charges accrue monthly unless otherwise stated.


### 3.1 Imported Amounts from CTA Template (Vendor L1)

#### Extracted Amount Lines

- Use these NSW-AUD figures (GST extra) as practical starting points.

### 3.2 Standard SKUs and Unit Prices (Vendor L1)
> Replace or supplement with the imported CTA tables as needed.

| SKU Code | SKU Name | Unit | Tier 1 (0–100) | Tier 2 (101–500) | Tier 3 (501–2,000) | Tier 4 (2,001+) | Notes |
|---|---|---|---:|---:|---:|---:|---|
| RUN-BASE | Platform Runtime | per tenant / month | A$[●] | A$[●] | A$[●] | A$[●] | Minimum 1 tenant |
| RUN-USR | Active Users | per user / month | A$[●] | A$[●] | A$[●] | A$[●] | Billed on monthly peak |
| API-100K | API Calls | per 100k calls | A$[●] | A$[●] | A$[●] | A$[●] | Rate limit applies |
| STRG-100 | Storage | per 100 GB / month | A$[●] | A$[●] | A$[●] | A$[●] | Metered on avg. GiB |
| SPT-L3 | Supplier L3 Support | per hour | A$[●] | — | — | — | Ad‑hoc under CTA |


## 4. Volume, Minimums, and Overage
4.1 **Minimum Commit.** Monthly minimums per SKU or overall monthly run fee as specified in the CTA.  
4.2 **Counting Method.** Users billed on monthly peak active users per tenant. API calls and storage billed on metered actuals.  
4.3 **Overage.** Usage above the highest fully discounted Tier is charged at the next applicable Tier rate.  
4.4 **True‑Up.** Quarterly true‑up per CTA. Shortfalls to Minimum Commit are invoiced at the end of each quarter.  
4.5 **Grace Band.** Optional ±5% grace band on user counts may be specified in the CTA; outside the band overage applies.


## 5. CPI Indexation
5.1 **Index.** ABS CPI (All Groups, Sydney).  
5.2 **Frequency.** Annual on the CPI Adjustment Date.  
5.3 **Formula.** New Price = Max( Base Price × (1 + CPI%), Base Price × (1 + Floor%) ), up to Cap% if specified.  
5.4 **Floor/Cap.** Unless stated in the CTA: Floor = 0%, Cap = 7% per annum. Negative CPI results in 0% change.  
5.5 **Publication Lag.** If CPI not published by the Adjustment Date, apply on publication with effect from the Adjustment Date.  
5.6 **New SKUs.** Priced by reference to comparable SKUs and indexed thereafter.


## 6. GST and Rounding
6.1 **GST.** Prices exclude GST. GST is added at the prevailing rate and shown on tax invoices.  
6.2 **Rounding.** Unit prices rounded to two decimal places. Extended totals rounded to nearest cent.  
6.3 **Tax Invoices.** Issued by Supplier in compliance with Australian GST law.


## 7. Tiering Mechanics
7.1 **Seat Bands.** Tier is determined by total billable active users across all relevant tenants in the Vendor’s L1 portfolio, unless the CTA specifies per‑tenant tiering.  
7.2 **Mid‑Period Changes.** Tier changes apply from the next billing cycle. Upon material growth mid‑cycle, Supplier may move to the higher Tier pro‑rata.  
7.3 **Down‑Tiering.** Occurs at month‑end if sustained usage is below the lower band for the entire month.


## 8. Discounts and Promotional Credits
8.1 **Stacking.** Promotional credits do not stack with contractual discounts unless the CTA states otherwise.  
8.2 **Expiry.** Promotional credits expire on the earlier of the date stated or the Term end.  
8.3 **Most‑Favoured Tier.** Vendor L1 receives the best applicable Tier for which it qualifies. No retroactive price protection.


## 9. Changes to the Price Book
9.1 **Process.** Supplier may update this Schedule with 30 days’ notice, subject to the CTA. Updated prices apply to new Orders immediately and to renewals or new usage at the next Renewal Term unless otherwise agreed.  
9.2 **No Reduction of Earned Discounts.** Existing committed Orders retain their contracted rates during their committed term.


## 10. Billing and Payment
10.1 **Invoicing.** Monthly in arrears unless otherwise stated in the CTA.  
10.2 **Currency.** AUD.  
10.3 **Late Payment.** Per CTA/MSA.  
10.4 **Stripe Sub‑Accounts.** Vendor L1 may configure its own Stripe account for downstream billing. Supplier bills Vendor L1 only.


## 11. Worked Example (Illustrative)
Assume RUN-USR Tier 2 price A$25.00 per user per month, 420 active users. Monthly charge = 420 × A$25.00 = A$10,500.00 + GST. If CPI% = 3.2% at next Adjustment Date and Floor/Cap per Section 5, new Tier 2 price = A$25.00 × 1.032 = A$25.80.


## 12. Order of Precedence
In case of conflict: CTA overrides this Schedule; this Schedule overrides the MSA and other documents, except as required by law.

---

**Execution**  
This Schedule is executed as an attachment to the CTA.

Vendor (L1): _Avanoua Pty Ltd_  
Authorised Signatory: __________________  Date: ___________

Supplier: _RePass Cloud Pty Ltd_  
Authorised Signatory: __________________  Date: ___________
