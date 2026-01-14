---
title: Event Families
description: Standard EVT patterns used across the platform and what they represent.
categories: [platform, logging]
tags: [events, conventions]
weight: 40
---

## Event families

Event families group related events into predictable naming patterns.
This makes logs easy to discover and easy to alert on.

The platform prefers:
- predictable prefixes (`UI_`, `API_`, `INT_`, `AUTO_`, `WF_`, `SEC_`, `DATA_`, `SYS_`)
- phase suffixes (`_START`, `_END`, `_ERR`, `_TIMEOUT`, `_RETRY`)

See the pages in this section for each family.
