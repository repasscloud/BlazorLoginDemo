---
title: Prefix Reference
description: Canonical prefix catalogue for platform identifiers (entity IDs and operational artefacts)
categories: [platform, identifiers]
tags: [prefixes, identifiers]
type: docs
---

## Overview

Prefixes are the primary human-friendly mechanism for identifying the *class* of an identifier.

Prefixes are:

- **Lowercase**
- **Stable** (never reassigned)
- **Short** (3–4 characters plus underscore)
- **Semantics only** (the random portion remains entropy-only)

Format:

```
<prefix>_<random>
```

## Prefix catalogue (platform)

| Prefix  | Refers to |
|--------|-----------|
| `org_` | Organization (Unified) |
| `dom_` | Organization Domain (Unified) |
| `lic_` | ClientLicense |
| `job_` | QueuedJob |
| `log_` | SystemLog |
| `err_` | Error Code (Unified) |
| `fxs_` | Exchange Rate Snapshot |
| `epol_` | Expense Policy |
| `tpol_` | Travel Policy |
| `etp_` | Ephemeral Travel Policy |
| `fvo_` | Flight View Option |
| `fosr_` | Flight Offer Search Request DTO |
| `fos_` | Flight Offer Search Result Record |
| `tqu_` | Travel Quote User |

## Rules

### Prefix meaning
- Prefix indicates what the ID refers to (entity/artefact class).
- Prefix does not encode tenant, region, environment, or vendor.

### Random portion meaning
- The random portion contains **no embedded meaning**
- No timestamps, no shard keys, no region codes
- Any additional metadata is stored in model fields, not inside the ID

### Operational parsing
- Support tooling and log queries may extract the prefix using the first underscore.
- Systems must treat the remaining portion as opaque.

### Adding a new prefix
When introducing a new entity/artefact identifier:
1. Select a unique prefix (3–4 chars) that does not collide with existing prefixes
2. Choose a random length (>=12) based on lifecycle and blast radius
3. Document it here and in **Identifier Types & Usage**
