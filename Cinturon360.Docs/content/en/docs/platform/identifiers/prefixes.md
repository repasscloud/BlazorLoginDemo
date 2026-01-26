---
title: Identifier Prefix Reference
description: Canonical list of identifier prefixes used across the platform
categories: [platform, identifiers]
---

## Overview

This page provides a **canonical reference** of all identifier prefixes used in the platform.

Prefixes are:
- Lowercase
- Short (3–4 characters)
- Stable
- Never reused for different entity types

They are intended to be parsed by humans and systems.

---

## Prefix List

| Prefix  | Entity |
|--------|--------|
| `org_` | Organization (Unified) |
| `dom_` | Organization Domain |
| `lic_` | ClientLicense |
| `job_` | QueuedJob |
| `log_` | SystemLog |
| `err_` | Error Code (Unified) |
| `fxs_` | Exchange Rate Snapshot |
| `epol_` | Expense Policy |
| `tpol_` | Travel Policy |
| `etp_` | Ephemeral Travel Policy |
| `fvo_` | Flight View Option |
| `fosr_` | Flight Offer Search Request DTO |
| `fos_` | Flight Offer Search Result Record |
| `tqu_` | Travel Quote User |

---

## Rules

- Prefixes convey **semantic meaning only**
- Random portions carry **no embedded meaning**
- Prefixes must not encode environment, region, or tenant
- New prefixes must be documented here before use
