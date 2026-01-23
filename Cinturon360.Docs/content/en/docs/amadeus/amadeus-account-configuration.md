---
title: Account Configuration
description: TMC-facing configuration fields required to access Amadeus pricing, booking, and ticketing services
categories: [amadeus]
tags: [amadeus, plugins]
weight: 2
---

_Last updated: 24 January 2026_

This page describes the commercial and configuration details required to connect a Travel Management Company (TMC) to the Amadeus API.

The values documented here are captured via the Amadeus configuration UI during onboarding or account setup.  
They can also be accessed by navigating to **Org QuickView** and selecting **Amadeus Account** from the header of the **License Agreement** section.

The information in this section is **commercial and configuration-focused only**.  
It does not describe implementation details or internal system architecture.

## Office ID

### What it is
The Amadeus Office ID identifies your commercial and ticketing office within Amadeus.

### What it controls
- Which fares and corporate agreements are returned  
- Which airlines you are authorised to price and ticket  
- The commercial context used for bookings

### Notes for TMCs
- This value is provided by Amadeus and is unique to your agency or branch.  
- Using the wrong Office ID may result in incorrect pricing or booking failures.

## Country Code

### What it is
The ISO country code that represents your Amadeus point of sale.

### What it controls
- Market-specific fares and rules  
- Regulatory and commercial constraints  
- Tax and pricing logic applied by Amadeus

### Notes for TMCs
- This is typically the country where the agency is registered or primarily operates (for example: AU, GB, US).

## Default Currency

### What it is
The currency used by default for pricing and displaying flight offers.

### What it controls
- The currency returned in search results  
- The base currency used for fare comparisons

### Notes for TMCs
- This does not prevent users from seeing other currencies.  
- It defines the default pricing context for searches and comparisons.

## Ticketing Enabled

### What it is
A flag indicating whether this Amadeus account is authorised to issue tickets, not just search for prices.

### What it controls
- Whether ticketing workflows are permitted  
- Whether bookings can progress beyond pricing and quoting

### Notes for TMCs
- If disabled, the system allows searches and quotes but blocks ticket issuance.

## Default Plating Carrier

### What it is
The airline code used as the default ticket plating carrier.

### What it controls
- Which airline issues the ticket stock  
- How revenue and commissions are allocated

### Notes for TMCs
- This is only required if your agreements mandate a specific plating carrier.  
- If left empty, Amadeus will determine the plating carrier automatically.

## Ticket Prefix

### What it is
The numeric prefix associated with your ticket stock.

### What it controls
- Validation of issued ticket numbers  
- Airline ownership of the ticket

### Notes for TMCs
- This is typically a three-digit number assigned by IATA or the airline.  
- Only required for agencies issuing their own ticket stock.

## API Endpoint (Environment)

### What it is
The Amadeus API environment this account connects to.

### What it controls
- Whether the system uses test/sandbox or production Amadeus services  
- Which Amadeus backend processes requests

### Notes for TMCs
- This is normally set to the production Amadeus endpoint.  
- Sandbox environments are used for testing and onboarding only.

## Summary

Each Amadeus account configuration defines:

- **Who you are** — Office ID and country  
- **How pricing is calculated** — market and default currency  
- **What you are allowed to do** — search-only or full ticketing  
- **How tickets are issued** — plating carrier and ticket prefix  
- **Which Amadeus system is used** — API environment

All values are provided by Amadeus or agreed as part of your commercial setup.
