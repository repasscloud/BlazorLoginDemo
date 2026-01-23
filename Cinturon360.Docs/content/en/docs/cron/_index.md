---
title: Cron & Scheduler
description: Containerised cron execution and operational scheduling
weight: 30
---

_Last updated: 24 January 2026_

This section documents the **cron container** used by Cinturon360 to execute scheduled and recurring operational jobs.

The container is intentionally minimal:
- No application logic
- No business rules
- Only **time-based orchestration**

Cron is treated as **infrastructure**, not application code.
