---
title: Account Configuration
description: TMC-facing configuration fields required to access Amadeus pricing, booking, and ticketing services
categories: [amadeus]
tags: [amadeus, tmc, external-api]
weight: 2
---

The values documented here are captured via the Amadeus configuration UI during onboarding or account setup.  
They can also be accessed by navigating to **Org QuickView** and selecting **Amadeus Account** from the header of the **License Agreement** section.

This content is **commercial and configuration-focused only**. It does not describe implementation details or internal system architecture.

## Office ID

Identifies your commercial and ticketing office within Amadeus.

This value determines:
- Which fares and corporate agreements are returned
- Which airlines you are authorised to price and ticket
- The commercial context used for bookings

Provided directly by Amadeus and unique to your agency or branch.  
Using an incorrect Office ID may result in incorrect pricing or booking failures.

## Country Code

Represents your Amadeus point of sale using an ISO country code.

This affects:
- Market-specific fares and rules
- Regulatory and commercial constraints
- Tax and pricing logic applied by Amadeus

Typically reflects the country where the agency is registered or primarily operates (for example: AU, GB, US).

## Default Currency

Defines the default currency used when pricing and displaying flight offers.

This controls:
- The currency returned in search results
- The base currency used for fare comparisons

Other currencies may still be visible, but this sets the default pricing context.

## Ticketing Enabled

Indicates whether the Amadeus account is authorised to issue tickets.

When enabled:
- Ticketing workflows are permitted
- Bookings can progress beyond pricing and quoting

When disabled:
- Searches and quotes are allowed
- Ticket issuance is blocked

## Default Plating Carrier

Specifies the airline code used as the default ticket plating carrier.

This influences:
- Which airline issues the ticket stock
- How revenue and commissions are allocated

Only required where commercial agreements mandate a specific plating carrier.  
If not specified, Amadeus will determine the plating carrier automatically.

## Ticket Prefix

The numeric prefix associated with your ticket stock.

Used for:
- Validation of issued ticket numbers
- Identifying airline ownership of the ticket

Typically a three-digit number assigned by IATA or the airline.  
Only required for agencies issuing their own ticket stock.

## API Endpoint (Environment)

Defines which Amadeus API environment the account connects to.

This determines:
- Whether requests are sent to sandbox or production services
- Which Amadeus backend systems process requests

Normally set to the production environment.  
Sandbox environments are used for testing and onboarding only.

## Summary

An Amadeus account configuration defines:

- **Identity and market context** — Office ID, country, currency
- **Operational capability** — search-only or full ticketing
- **Ticket issuance rules** — plating carrier and ticket prefix
- **System environment** — API endpoint

All values are supplied by Amadeus or agreed as part of your commercial setup.
