---
title: Standing Payment Authorization
description: Default standing payment authorization template and guidance for off-session 'as-required' charging for approved travel charges.
categories: [platform, payments]
tags: [authorization, off-session, template]
weight: 50
---

_Last updated: 24 January 2026_

# Standing Payment Authorization for Approved Travel Charges

## Terminology

This legal language is commonly described using industry-standard commercial and card-network terminology:

- **Payment Authorization** (also called **Card Authorization** or **Authorization to Charge**): the client authorizes the merchant to charge a payment method.
- **Standing / Ongoing Authorization** (also called **Standing Payment Authorization**, **Ongoing Payment Authorization**, **Continuing Authorization**, or **Stored Payment Authorization**): the authorization remains in effect and can be used for future charges.
- **Authorization for Off-Session Charges** (also called **Authorization for Future Charges Without Cardholder Present** or **Merchant-Initiated Transaction Authorization**): consent to charges where the cardholder is not actively completing the payment at the time of charge.
- **Purpose-Limited Payment Authorization** (also called **Authorization for Approved Travel Transactions** or **Authorization for Contractually Approved Charges**): the authorization is limited to specific categories of charges under the parties' agreement.

## Recommended naming

Use the following heading or title in the UI and in legal text:

- **Standing Payment Authorization for Approved Travel Charges**

Suggested internal naming (optional, for consistency):

- **Audit log action:** Standing payment authorization accepted
- **Internal model key:** StandingPaymentAuthorization
- **Payment processor mapping:** off_session = true

## Placeholders used in this template

Replace placeholders using your preferred variable system. Typical placeholders:

- `{TMC_Legal_Name}`
- `{TMC_Trading_Name}`
- `{Client_Legal_Name}`
- `{Client_Trading_Name}`
- `{Platform_Name}`
- `{Payment_Processor}` (example: Stripe)
- `{Merchant_Of_Record}` (example: `{TMC_Legal_Name}` or another entity)
- `{Statement_Descriptor}` (example: `{TMC_Trading_Name}` or a descriptor provided by `{Payment_Processor})
- `{Agreement_Name}`
- `{Agreement_Date}`
- `{Effective_Date}`
- `{Currency_Code}` (example: AUD)
- `{Notice_Channel}` (example: email, portal notification)
- `{Support_Email}`
- `{Dispute_Notice_Period_Days}` (example: 14)
- `{Revocation_Channel}` (example: written notice to `{Support_Email}` or via portal)

Use only the placeholders you have data for. Unused placeholders can be removed.

## Short-form UI copy

### Heading

Standing Payment Authorization for Approved Travel Charges

### Body

By proceeding, `{Client_Legal_Name}` authorizes `{TMC_Legal_Name}` to store and use the selected payment method for **off-session** charges for **approved travel** under the agreement between `{TMC_Legal_Name}` and `{Client_Legal_Name}` (`{Agreement_Name}`, dated `{Agreement_Date}`).

This is a **standing** (ongoing) authorization and may be used for future approved travel transactions, including applicable fees, taxes, and carrier or supplier surcharges.

Payment details are processed and stored by `{Payment_Processor}` in a tokenized form. `{Platform_Name}` does not store full card details.

You can revoke this authorization at any time through `{Revocation_Channel}`. Revocation does not affect charges already incurred or bookings already made.

### Checkbox label

I authorize ongoing off-session charges for approved travel in accordance with the `{TMC_Legal_Name}` - `{Client_Legal_Name}` agreement.

## Full-form terms

### 1. Parties and scope

This Standing Payment Authorization for Approved Travel Charges (Authorization) is given by **`{Client_Legal_Name}`** (`{Client_Trading_Name}`, Client) to **`{TMC_Legal_Name}`** (`{TMC_Trading_Name}`, TMC), effective from **`{Effective_Date}`**.

This Authorization applies to travel and travel-related services arranged by the TMC for or on behalf of the Client, where those services are requested, approved, or otherwise permitted under the agreement between the TMC and the Client (Agreement).

### 2. Definitions

For this Authorization:

- **Agreement** means `{Agreement_Name}` dated `{Agreement_Date}` (as amended from time to time).
- **Approved Travel Charge** means a charge for travel or travel-related services that is permitted under the Agreement, including a charge that is (a) expressly approved by the Client under the Client's approval workflow, or (b) deemed approved or auto-approved under the Agreement or the Client's policy settings or instructions to the TMC.
- **Payment Method** means the card or other payment method provided by or on behalf of the Client and selected as the default method for charges under this Authorization.
- **Off-session** means a charge where the cardholder is not actively completing the transaction at the time the charge is processed.

### 3. Standing Payment Authorization

The Client authorizes the TMC (and any entity acting on behalf of the TMC that is required to process payment, including the Payment Processor) to:

1. create and maintain a stored payment credential for the Payment Method (including tokenization and storage by the Payment Processor);
2. charge the Payment Method on a **standing** (ongoing) basis for Approved Travel Charges; and
3. submit and process those charges as **off-session** transactions where applicable.

The Client acknowledges that Approved Travel Charges may occur after a booking is created (for example, at time of ticketing, supplier confirmation, changes, cancellations, no-shows, or post-trip adjustments) and may be processed without the cardholder being present.

### 4. Purpose limitation

This Authorization is **purpose-limited**. The TMC may only charge the Payment Method for Approved Travel Charges. The TMC must not use this Authorization for unrelated goods or services.

Approved Travel Charges may include (to the extent permitted by the Agreement):

- supplier charges (airlines, hotels, ground transport, rail, hire vehicles, activities, and other travel suppliers);
- TMC service fees, booking fees, change fees, cancellation fees, management fees, and other fees payable under the Agreement;
- taxes, duties, and government charges; and
- carrier or supplier surcharges and pass-through charges.

### 5. Amounts, timing, and preauthorizations

The Client acknowledges that:

- charges may be processed in the currency specified in the Agreement or displayed at the time of booking (for example, `{Currency_Code}`);
- the final amount may vary from a quoted amount due to supplier rules, availability, exchange rates, changes, upgrades, penalties, taxes, and other adjustments permitted under the Agreement; and
- payment networks and suppliers may place preauthorizations, incremental authorizations, or reversals as part of normal processing.

Where the TMC is able to do so, the TMC will provide receipts, invoices, or charge notices through `{Notice_Channel}`.

### 6. Payment processing, merchant of record, and data handling

Charges are processed using **`{Payment_Processor}`**. The Payment Method may be stored and managed by `{Payment_Processor}` in a tokenized form.

- **Merchant of record:** `{Merchant_Of_Record}`.
- **Statement descriptor:** Charges may appear on statements using a descriptor such as `{Statement_Descriptor}`.
- `{Platform_Name}` does not store full card numbers (PAN) or card security codes (CVC/CVV).
- The Client acknowledges that the Payment Processor, card schemes, and issuing banks may apply their own terms, security checks, and processing rules.

### 7. Authority and confirmation

The person accepting this Authorization on behalf of the Client represents and warrants that they are authorized to:

- bind the Client to this Authorization and the Agreement;
- provide the Payment Method for use for Approved Travel Charges; and
- request travel services and approve (or set up auto-approval for) charges under the Agreement.

### 8. Disputes, chargebacks, and errors

If the Client believes a charge is incorrect, the Client must notify the TMC promptly (and in any event within `{Dispute_Notice_Period_Days}` days of the charge date, unless the Agreement specifies a different period) and provide reasonable details to allow investigation.

- Where the issue relates to supplier services (for example, airline or hotel charges), the Client acknowledges that resolution may depend on the supplier's rules and evidence.
- To the extent permitted by law and the Agreement, the Client must not initiate a chargeback for an Approved Travel Charge without first giving the TMC a reasonable opportunity to investigate and respond.

Nothing in this clause limits any rights the Client has under applicable law that cannot be excluded.

### 9. Revocation, replacement, and suspension

The Client may revoke this Authorization at any time via `{Revocation_Channel}`. Revocation:

- takes effect only after it is received and processed by the TMC; and
- does not affect charges already incurred, bookings already made, supplier penalties, or amounts payable under the Agreement.

If the Client revokes this Authorization or the Payment Method becomes invalid, the TMC may (subject to the Agreement) suspend new bookings and/or require an alternative payment method.

### 9A. Refunds, credits, and reversals

Where a refund, credit, or reversal is due under the Agreement or supplier rules:

- the TMC may (where permitted) refund amounts back to the original Payment Method used for the charge, or apply a credit in accordance with the Agreement;
- timing of refunds may depend on supplier processing timeframes and payment network rules; and
- supplier penalties, cancellation fees, and non-refundable components may reduce the amount refundable.

### 10. Records and evidence

The Client agrees that the following may be used as evidence of Approved Travel Charges and the Client's consent:

- booking records, itineraries, approvals, and change/cancellation history;
- receipts and invoices issued by the TMC or suppliers;
- Payment Processor records (including token references and payment confirmations); and
- audit logs and system records showing acceptance of this Authorization.

### 11. Acknowledgment

By accepting this Authorization, the Client acknowledges that it is giving a **standing**, **off-session**, **purpose-limited** authorization for Approved Travel Charges under the Agreement.

### 12. Acceptance statement (for clickwrap)

By selecting "Accept" or checking the box, I confirm that I am authorized to accept this Standing Payment Authorization on behalf of `{Client_Legal_Name}` and I agree to the terms above.
