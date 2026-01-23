---
title: Security Logging
description: Authentication, authorisation, and privileged actions (SEC).
categories: [platform, logging]
tags: [security, audit]
weight: 90
---

_Last updated: 24 January 2026_

## Security events

Security logs support auditing and incident response.

Use `CAT=SEC` for:
- login attempts (success/failure)
- authorization denies
- role assignment/removal
- privileged access (sudo/impersonation)
- credential changes

### Required fields

- `UID` where available
- `RID`
- `TID`, `ORG`
- include `ROLE` if your model exposes it

### Standard events

- `SEC_LOGIN_OK`
- `SEC_LOGIN_FAIL`
- `SEC_DENY`
- `SEC_ROLE_CHANGE`
- `SEC_IMPERSONATION_START`
- `SEC_IMPERSONATION_END`
