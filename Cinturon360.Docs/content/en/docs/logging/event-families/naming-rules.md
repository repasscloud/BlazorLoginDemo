---
title: Event Naming Rules
description: How to create EVT codes that remain stable and searchable over time.
categories: [platform, logging]
tags: [events, naming]
weight: 10
---

_Last updated: 24 January 2026_

## EVT naming rules

### Required properties

- Uppercase
- Underscore separated
- Stable over time
- Does not include identifiers (IDs belong in fields)
- Represents one event only (do not reuse EVT for different meanings)

### Recommended patterns

| Pattern | Meaning | Examples |
|---|---|---|
| `{FAMILY}_{THING}_{PHASE}` | lifecycle events | `API_REQ_END`, `INT_CALL_START` |
| `UI_{ACTION}_{DETAIL}` | UI traceability | `UI_PAGE_OPEN`, `UI_SAVE_PRESSED` |
| `SEC_{ACTION}_{RESULT}` | security | `SEC_LOGIN_OK`, `SEC_LOGIN_FAIL` |
| `AUTO_JOB_{PHASE}` | jobs | `AUTO_JOB_START`, `AUTO_JOB_FAIL` |
| `WF_STEP_{PHASE}` | workflows | `WF_STEP_START`, `WF_STEP_END` |

### Avoid

- long paragraphs as event codes
- including route or entity IDs inside the EVT
- mixing multiple outcomes inside one EVT (emit separate events)
