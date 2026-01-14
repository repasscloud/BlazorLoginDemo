---
title: Log Levels
description: Meaning of Level and when to use each severity.
categories: [platform, logging]
tags: [taxonomy, levels]
weight: 40
---

## Level (Severity)

| Level | Use | Notes |
|---|---|---|
| `Verbose` | very fine diagnostics | typically disabled in production |
| `Debug` | developer-focused troubleshooting | safe in lower environments |
| `Information` | lifecycle + business events | default for traceability |
| `Warning` | degraded behaviour | retries, partial failures |
| `Error` | operation failed | include exception when applicable |
| `Fatal` | process compromised | usually paired with shutdown/exit |

### Platform guidance

- Use `Information` for **user intent** logs (page open, save pressed, function executed).
- Use `Warning` when the system is still functioning but requires attention.
- Use `Error` when the operation did not complete.
- Use `Fatal` for one-off “we are exiting / cannot continue” scenarios.
