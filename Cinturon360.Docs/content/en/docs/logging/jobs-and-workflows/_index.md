---
title: Jobs and Workflows
description: Logging for background jobs (AUTO) and multi-step workflows (WF).
categories: [platform, logging]
tags: [jobs, workflows]
weight: 80
---

## Jobs (AUTO)

Jobs are scheduled or queued units of work.
Use `AUTO_*` events for job lifecycle and retry tracking.

Standard events:
- `AUTO_JOB_ENQUEUED`
- `AUTO_JOB_START`
- `AUTO_JOB_END`
- `AUTO_JOB_FAIL`
- `AUTO_RETRY`

## Workflows (WF)

Workflows are multi-step processes (Validate -> Price -> Hold -> Ticket).
Use `WF_*` events for step breadcrumbs.

Standard events:
- `WF_STEP_START`
- `WF_STEP_END`
- `WF_ERR`
- `WF_COMPENSATE`

### Required correlation

Jobs and workflows must preserve:
- `RID` (from the originating request/user action)
- `TID`, `ORG`
- `UID` when the job is user-initiated (store it in job payload if needed)
