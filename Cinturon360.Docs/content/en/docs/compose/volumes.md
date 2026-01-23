
---
title: Volumes & shared state
description: Persistent and shared storage in Compose
weight: 50
---

_Last updated: 24 January 2026_

This page documents all **named volumes** used by the Compose stack.

Values and contents are **not** shown. This documentation describes **intent, lifecycle, ownership, and safety** only.

## Overview

Named volumes are used in Compose to:

- Persist critical state across container restarts
- Share state between cooperating containers
- Cache build artefacts for performance
- Isolate sensitive runtime material (keys, certificates)

Volumes are **explicitly declared** to avoid accidental data loss and to make lifecycle decisions visible.

## Database volumes

### `postgresql-data`

**Purpose**  
Persistent PostgreSQL cluster storage.

**Used by**
- `db`

**Contains**
- Database files
- WAL segments
- Internal PostgreSQL metadata

**Lifecycle**
- Long‑lived
- Persists across container rebuilds and restarts

**Safe to delete?**
- ❌ No  
  Deleting this volume destroys all database data.

### `pgadmin-data`

**Purpose**  
Persistent pgAdmin UI and configuration state.

**Used by**
- `pgadmin`

**Contains**
- Saved server connections
- User preferences
- Session state

**Lifecycle**
- Optional persistence
- Can be recreated without affecting application data

**Safe to delete?**
- ⚠️ Yes  
  pgAdmin will start fresh and require reconfiguration.

## Application cryptography volumes

### `api-dataprotection`

**Purpose**  
ASP.NET Data Protection key ring for the API container.

**Used by**
- `api`

**Contains**
- Cryptographic keys used for:
  - Cookie encryption
  - Token protection
  - Data protection APIs

**Lifecycle**
- Must persist across restarts
- Required to avoid invalidating active sessions

**Safe to delete?**
- ⚠️ Only if you intend to invalidate all existing API tokens/sessions.

### `web-dataprotection`

**Purpose**  
ASP.NET Data Protection key ring for the web application.

**Used by**
- `webapp`

**Contains**
- Cryptographic keys for:
  - Authentication cookies
  - CSRF protection
  - Encrypted UI state

**Lifecycle**
- Must persist across restarts

**Safe to delete?**
- ⚠️ Only if user sessions may be safely invalidated.

## Shared temporary storage

### `sharedtmp`

**Purpose**  
Shared temporary file storage between containers.

**Used by**
- `api`
- `tmp-cleaner`

**Contains**
- Temporary, non‑durable files created by the API
- Intermediate artefacts not intended for persistence

**Lifecycle**
- Short‑lived by design
- Actively cleaned on a schedule

**Maintenance**
- The `tmp-cleaner` container removes files from this volume every **3600 seconds**
- Prevents unbounded disk growth
- Enforces ephemeral usage semantics

**Safe to delete?**
- ✅ Yes  
  Files are temporary and recreated as needed.

## Documentation build caches

### `docsy_node_modules`

**Purpose**  
Cached Node.js dependencies for the Docsy documentation container.

**Used by**
- `docs`

**Contains**
- `node_modules` directory

**Lifecycle**
- Performance optimisation only

**Safe to delete?**
- ✅ Yes  
  Dependencies will be reinstalled.

### `docsy_go_cache`

**Purpose**  
Go module cache for Hugo / Docsy.

**Used by**
- `docs`

**Contains**
- Go build cache
- Downloaded modules

**Lifecycle**
- Performance optimisation only

**Safe to delete?**
- ✅ Yes

### `docsy_hugo_cache`

**Purpose**  
Hugo build cache.

**Used by**
- `docs`

**Contains**
- Generated artefacts
- Render cache

**Lifecycle**
- Performance optimisation only

**Safe to delete?**
- ✅ Yes

## Volumes not currently in use

### `caddy-data`
### `caddy-config`

These volumes are **declared but not used**.

**Reason**
- Caddy is not part of the current Compose stack
- NGINX with Let’s Encrypt is used in **UAT and production**
- Local Compose currently connects directly to containers

These volumes may be removed or repurposed in the future.

## Design principles

- Persistent data is explicit and protected
- Temporary data is aggressively cleaned
- Crypto material is isolated per service
- Caches are safe to destroy at any time

This separation prevents accidental data loss while keeping local development fast.
