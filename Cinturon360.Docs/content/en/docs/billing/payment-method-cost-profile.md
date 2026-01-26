---
title: Payment Method Cost Profile
type: docs
weight: 30
---

## What this is

A **Payment Method Cost Profile** is a system-maintained classification of the saved payment method used to pay invoices.

It exists so the platform can:

- predict payment processing costs **before charging**
- calculate fees deterministically
- avoid margin leakage

This profile is:

- calculated automatically
- updated by background services
- read-only in the UI
- never edited by users

## Why this exists

Payment processors charge different fees based on:

- card brand (Visa, MasterCard, AMEX, JCB, UnionPay, etc.)
- issuing country
- funding type (credit / debit / prepaid)

You cannot safely calculate platform fees without knowing these characteristics.

## What is stored

Typical fields include:

- Card brand
- Card issuing country (ISO-2)
- Card funding type
- Derived billing fee tier
- Convenience flags (domestic, high-cost)
- Last 4 digits (display only)
- Card fingerprint (change detection)
- Last updated timestamp

No PANs or sensitive data are stored.

## Why it is read-only

Allowing manual edits would:

- break pricing guarantees
- create audit risk
- introduce inconsistent state

All updates are system-owned.
