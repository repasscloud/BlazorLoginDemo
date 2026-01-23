---
title: Expiry job
description: Hourly expiry and cleanup trigger
weight: 10
---

_Last updated: 24 January 2026_

## Purpose

The expiry job triggers server-side logic responsible for enforcing time-based rules.

Cron does **not** implement expiry logic itself.

## Responsibilities

- Expire stale bookings or drafts
- Clean up temporary state
- Enforce time-based invariants

## Design notes

- Safe to re-run
- Idempotent by contract
- All business logic lives behind the API endpoint
