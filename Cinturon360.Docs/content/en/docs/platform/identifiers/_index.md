---
title: Identifiers
description: Identifier standards, prefixes, and lifecycle rules used across the Cinturon360 platform
categories: [platform, identifiers]
---

## Overview

Cinturon360 uses **prefixed, high-entropy identifiers** across the platform to uniquely identify
entities, policies, operational records, and transient artefacts.

Identifiers are designed to be:

- Globally unique within their scope
- Human-readable in logs and diagnostics
- Safe for URLs, databases, and exports
- Collision-resistant without coordination
- Explicit about *what* they identify via prefixes

This section documents:
- Identifier structure
- Prefix conventions
- Length and lifecycle considerations
- Entity-specific identifier usage

## Identifier Structure

All identifiers follow this canonical form:

```
<prefix>_<random>
```

Where:
- `prefix` conveys semantic meaning
- `_` is a hard delimiter
- `random` is a NanoID-generated value with no embedded meaning

Different entities use different **prefixes** and **lengths** based on lifecycle and risk profile.

## Pages in this section

- **Identifier Types & Usage** – entity-by-entity explanation of identifiers
- **Identifier Prefix Reference** – canonical list of all prefixes
