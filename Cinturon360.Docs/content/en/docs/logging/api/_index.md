---
title: API Logging
description: How API requests are logged, including request lifecycle, validation, and controller actions.
categories: [platform, logging]
tags: [api, http]
weight: 60
---

## API request lifecycle

API logs represent the lifecycle of HTTP requests and their outcomes.

The recommended baseline is:
- `API_REQ_START` (optional in high-volume scenarios)
- `API_REQ_END` (required)
- `API_VALIDATION_FAIL` (required when validation fails)
- `API_REQ_ERR` (required for unhandled errors)

### Required correlation

All API logs must include:
- `RID` (correlation id)
- `TID` and `ORG` (tenant context)
- `UID` where authenticated
- `HTTP`, `STAT`, `PATH`

The same `RID` must be propagated into worker jobs and integration calls that originate from the request.
