---
title: Identifiers
description: Identifier standards, prefixes, lengths, and collision-handling rules used across the Cinturon360 platform
categories: [platform, identifiers]
tags: [platform, identifiers]
type: docs
---

## Purpose

Identifiers in Cinturon360 are designed to be:

- **Unambiguous** in logs and support tooling (prefix + delimiter)
- **Safe** for URLs, databases, exports, and integrations
- **Collision-resistant** without coordination (NanoID entropy)
- **Predictable** in format across all modules (`<prefix>_<random>`)

This section is written for **DevTeam**, **Product Support**, **Vendors**, and **TMCs**.

## Canonical format

All platform identifiers follow:

```
<prefix>_<random>
```

- `prefix` conveys *what* the identifier refers to (entity / artefact class)
- `_` is a hard delimiter for simple parsing
- `random` is a NanoID value (entropy only; no embedded meaning)

## Generation & safety model

Cinturon360 uses **probabilistic uniqueness** (NanoID) plus **deterministic enforcement** (database uniqueness where applicable):

- NanoID provides a huge keyspace (low collision probability)
- **Unique constraints / indexes** make collisions non-events (retry on conflict)
- Length is selected based on **lifetime**, **volume**, and **blast radius** of a collision


