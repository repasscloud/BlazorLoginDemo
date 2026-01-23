---
title: Integrations Logging
description: "How provider calls are logged: timing, retries, outcomes, mapping failures."
categories: [platform, logging]
tags: [logging, integrations]
weight: 70
---

_Last updated: 24 January 2026_

## Integration call logging

Integration logs cover calls to external providers (GDS, payments, messaging, etc.).

The platform standardises:
- `INT_CALL_START`
- `INT_CALL_END`
- `INT_ERR`
- `INT_TIMEOUT`
- `INT_RETRY`

### Required fields

- `PROV` (provider name)
- `RID` (correlation id)
- `TID`, `ORG` (tenant context)
- `STAT` where available (HTTP provider status or mapped status)
- `DUR`

### Mapping failures

If provider response mapping fails (unexpected schema, missing data), log:
- `EVT=INT_ERR`
- `OUT=ERR` or `FAIL` depending on impact
- include `NOTE=mapping_failed` and relevant high-level clues (never secrets)
