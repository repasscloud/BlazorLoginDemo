---
title: Categories
description: Meaning of each CAT value and what belongs in it.
categories: [platform, logging]
tags: [taxonomy, categories]
weight: 10
---

_Last updated: 24 January 2026_

## CAT (Category)

`CAT` identifies **where** the event happened.

| CAT | Meaning | Typical events |
|---|---|---|
| `UI` | User interface activity | page open, button pressed, edit mode |
| `API` | HTTP API lifecycle | request start/end, validation, controller action |
| `SEC` | Security and access control | login, deny, role changes |
| `DATA` | Data layer actions | writes, migrations, critical reads |
| `INT` | External integration calls | provider start/end, timeout, mapping failures |
| `AUTO` | Background jobs and schedulers | job start/end, retry, failure |
| `WF` | Multi-step workflows | step start/end, compensation |
| `SYS` | Host/platform health | health checks, startup, shutdown |

### Category selection rules

- If the event originates from an HTTP request handler, prefer `API` unless it is explicitly UI instrumentation.
- Provider calls are always `INT` even if initiated from `API`.
- Auth failures and permission checks are `SEC`.
- Long-running multi-step flows use `WF` for step-level events and `AUTO` for job-level events.
