---
title: Standard API Events
description: Standard EVT codes for API request start/end/errors and validation failures.
categories: [platform, logging]
tags: [logging, api]
weight: 10
---

## API_REQ_END

Emitted at the end of request processing.

| Field | Value |
|---|---|
| `CAT` | `API` |
| `ACT` | `READ/CREATE/UPDATE/DELETE/EXEC` (choose based on endpoint intent) |
| `OUT` | derived from result (`OK`, `ERR`, `DENY`, `FAIL`) |
| `HTTP` | method |
| `STAT` | status |
| `PATH` | route |
| `DUR` | request duration |
| `ENT`, `EntId` | when the endpoint is primarily about a domain entity |

Example:
```
EVT=API_REQ_END CAT=API ACT=CREATE OUT=OK HTTP=POST STAT=201 PATH=/api/quotes RID=… TID=… UID=… DUR=84MS ENT=Quote EntId=q_123 CNT=1
```

## API_VALIDATION_FAIL

Use for validation failures (body missing, invalid field, domain constraints).

Outcome guidance:
- `OUT=ERR`
- use `Level=Warning` if you consider it operator-interesting, otherwise `Information`

Example:
```
EVT=API_VALIDATION_FAIL CAT=API ACT=CREATE OUT=ERR HTTP=POST STAT=400 PATH=/api/quotes RID=… NOTE=errorCode=validation_failed
```

## API_REQ_ERR

Unhandled exception path.

- `Level=Error`
- include exception
- `OUT=FAIL` if it breaks the request and you return 500

Example:
```
EVT=API_REQ_ERR CAT=API ACT=EXEC OUT=FAIL HTTP=POST STAT=500 PATH=/api/ticketing RID=… NOTE=unhandled_exception
```
