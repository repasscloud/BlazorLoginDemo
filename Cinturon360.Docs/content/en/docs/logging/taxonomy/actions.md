---
title: Actions
description: Meaning of ACT values (verb vocabulary).
categories: [platform, logging]
tags: [taxonomy, actions]
weight: 20
---

## ACT (Action)

`ACT` is a verb that describes what happened.

Recommended set (platform standard):

| ACT | Meaning | Examples |
|---|---|---|
| `VIEW` | user viewed something | page open, read-only route |
| `READ` | data read | retrieving entity |
| `CREATE` | new entity created | quote created |
| `UPDATE` | entity updated | company updated |
| `DELETE` | entity deleted | soft-delete |
| `EXEC` | function executed | test connection, reprice |
| `LOGIN` | authentication action | login attempt |
| `LOGOUT` | logout action | logout |
| `START` | lifecycle start | request/job start |
| `END` | lifecycle end | request/job end |
| `STEP` | workflow step | validate->price->ticket |
| `CLICK` | explicit UI click | button pressed (only if you choose to distinguish from EXEC/UPDATE) |

### Guidance

- Prefer `EXEC` for non-CRUD operations initiated by a user or system.
- Prefer `STEP` for workflow breadcrumb logs.
- Use `START`/`END` for paired lifecycle logs.
