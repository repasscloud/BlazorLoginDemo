---
title: Field Reference
description: Field-by-field meaning, examples, and guidance on correct usage.
categories: [platform, logging]
tags: [fields, reference]
weight: 20
---

_Last updated: 24 January 2026_

## Field-by-field reference

### Timestamp

- **Type:** UTC timestamp
- **Meaning:** time the event was recorded (not necessarily when it started)
- **Usage:** always set to UTC

### Level

- **Type:** `Verbose | Debug | Information | Warning | Error | Fatal`
- **Meaning:** severity (see taxonomy)

### EVT

- **Type:** string
- **Meaning:** stable event code
- **Rules:**
  - must be durable (do not rename casually)
  - must not include IDs (IDs go in fields)
  - should be searchable and descriptive

Examples:
- `UI_PAGE_OPEN`
- `API_REQ_END`
- `INT_CALL_END`
- `AUTO_JOB_FAIL`

### CAT

- **Type:** enum
- **Meaning:** where the event occurred (UI/API/SEC/INT/etc.)

### ACT

- **Type:** enum
- **Meaning:** verb describing the event
- **Good:** `VIEW`, `CREATE`, `UPDATE`, `DELETE`, `EXEC`, `LOGIN`, `START`, `END`, `STEP`
- **Avoid:** vague verbs like `DO`, `RUN`, `THING`

### OUT

- **Type:** enum
- **Meaning:** normalised outcome for filtering/alerting

Common outcomes:
- `OK` — expected success
- `WARN` — degraded but succeeded or recoverable
- `ERR` — failed operation
- `FAIL` — critical failure, operation could not complete
- `DENY` — blocked by authorization
- `TIMEOUT` — exceeded time budget
- `RETRY` — attempt will be retried
- `CANCEL` — user/system cancelled

### ENT and EntId

- **Type:** strings
- **Meaning:** target entity type and identifier
- **Rules:**
  - `ENT` is a noun (`Quote`, `Booking`, `Company`, `IntegrationCredential`)
  - `EntId` is the stable ID used by your system (GUID/string)
  - if the event is not about a domain entity, use `ENT=Page` or omit `ENT`

### RID

- **Type:** string
- **Meaning:** correlation id used to join events
- **Rules:**
  - must exist for every event
  - must propagate across service boundaries (API -> worker -> integration call)

### TID and ORG

- **Type:** string
- **Meaning:** multi-tenant partition fields
- **Rules:**
  - always set when a request is within tenant scope
  - for platform-wide/system events, set `TID`/`ORG` to a known platform value (not empty), or document the system convention

### UID

- **Type:** string
- **Meaning:** actor identifier (user)
- **Rules:**
  - use identity provider subject / stable user ID
  - do not log emails unless your identity model uses emails as stable ids (prefer not)

### DUR / DurMs

- **Type:** int
- **Meaning:** duration for the operation or span represented by the log
- **Rules:**
  - use milliseconds
  - for “start/end” paired events, prefer `DUR` on the end event

### HTTP / STAT / PATH

- **Meaning:** request metadata for API and UI routing
- **Rules:**
  - `PATH` should be the route template or resolved route consistently (pick one convention and follow it)
  - `STAT` is the numeric HTTP status code
  - `HTTP` is method

### NOTE

- **Meaning:** short hint for operators
- **Rules:**
  - never the only place critical info exists
  - keep it concise; use key-value style when possible (e.g. `errorCode=validation_failed`)
