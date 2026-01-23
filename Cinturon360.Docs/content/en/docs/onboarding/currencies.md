---
title: Currency & Billing
description: Application billing currency model, supported currencies, and exchange handling for TMC onboarding
categories: [application, billing, currency]
tags: [docs]
weight: 3
---

# Currency & Billing

This page documents how **currency**, **billing**, and **exchange handling** work within the application from a **Travel Management Company (TMC)** and **client** perspective.

The intent is to provide clear expectations during onboarding and ongoing operations.

---

## Base billing currency

- The **primary system currency is AUD (Australian Dollars)**.
- All internal calculations, settlements, and financial reporting are **anchored to AUD**.
- Unless explicitly configured otherwise, all client billing is assumed to be **AUD-based**.

AUD acts as the **settlement currency of record** for the platform.

---

## Supported billing currencies

Clients may be billed in the following currencies:

| Currency Code | Symbol | Currency |
|---|---|---|
| `AUD` | A$ | Australian Dollar |
| `USD` | US$ | United States Dollar |
| `CAD` | C$ | Canadian Dollar |
| `NZD` | NZ$ | New Zealand Dollar |
| `EUR` | € | Euro |
| `GBP` | £ | British Pound |
| `JPY` | ¥ | Japanese Yen |

These currencies are supported consistently across:
- Client invoices
- UI price displays
- Payment processor integrations

---

## Currency selection hierarchy (important)

The **effective billing currency** for a charge is resolved in the following order:

1. **Expense / Travel Policy settings**  
   - Policy rules *always take precedence*
   - Used for compliance and client-specific controls

2. **Client billing currency profile**  
   - Applied when no overriding policy exists

3. **TMC default billing currency**  
   - Defaults to **AUD**

> **Key rule:**  
> Expense and Travel Policy settings override the client currency profile.

---

## Built-in currency exchange

The application includes a **built-in currency exchange mechanism** that:

- Uses the **latest available exchange rates**
- Converts **foreign billing currencies → AUD** for settlement
- Is applied automatically when required

### Common scenarios

#### Client billed in AUD (default)
- Prices displayed and billed in AUD
- No currency conversion required

#### Client billed in foreign currency (e.g. EUR)
- Client invoice issued in **EUR**
- Amount converted and **settled in AUD** to the TMC’s account
- Conversion handled by the **payment processor**

---

## Payment processing model

### Stripe (optional, recommended)

- Stripe can be used during TMC onboarding
- Stripe handles:
  - Currency conversion
  - FX rates
  - Settlement to the TMC’s AUD account
- Simplifies multi-currency billing and reconciliation

### Custom / external payment processor

- TMCs may **opt out of Stripe entirely**
- A TMC can:
  - Integrate their **own payment processor**
  - Handle FX and settlement externally
- The application will still:
  - Enforce currency rules
  - Apply policy overrides
  - Provide correct billing currency values

> Stripe is **not mandatory** and can be skipped during onboarding.

---

## Settlement behaviour (summary)

| Client Billing Currency | TMC Account Currency | Settlement |
|---|---|---|
| AUD | AUD | Direct, no FX |
| EUR | AUD | FX conversion → AUD |
| USD | AUD | FX conversion → AUD |
| GBP | AUD | FX conversion → AUD |
| JPY | AUD | FX conversion → AUD |

Settlement currency remains **AUD** unless the TMC has made alternative arrangements with their payment processor.

---

## What clients will see

Clients will see:

- Prices displayed in their **effective billing currency**
- Invoices issued in that currency
- Consistent symbols and formatting
- No exposure to internal settlement currency (AUD)

All currency behaviour is transparent and policy-driven.

---

## Key takeaways

- AUD is the **system and settlement base currency**
- Multiple billing currencies are supported
- Expense / Travel Policy settings override everything else
- Stripe is optional
- FX is handled automatically when required
- TMCs retain full control over payment processing

---
