---
title: Tenancy and Access Models
type: docs
---

All Cinturon360 API access is scoped to a tenant context.  
The route namespace determines both access level and data visibility.

## TMC API Access (`/tmc`)

Travel Management Companies (TMCs):

- **MUST** request API access via their upstream vendor.
- Receive credentials scoped to their TMC tenant.
- **MUST** use the `/tmc` route namespace.

Example:

```
/v1/tmc/...
```

The `/tmc` namespace provides operational, configuration, and management-level access relevant to the TMC.

## Client API Access (`/client-data`)

Clients requesting API access:

- **MUST** request access through their TMC.
- Are restricted to reporting and read-only data access.
- **MUST** use the `/client-data` route namespace.

Example:

```
/v1/client-data/...
```

Clients do not receive access to operational or administrative endpoints.

## Reporting Formats

All reporting endpoints return one of the following formats only:

- **JSON** — structured, machine-readable data
- **PDF** — generated, human-readable reports

No other formats (CSV, XML, XLSX, etc.) are supported.

## Enforcement

- API access is not enabled by default.
- Access scope is enforced at authentication and authorization layers.
- Misuse or unauthorized access may result in credential revocation.
