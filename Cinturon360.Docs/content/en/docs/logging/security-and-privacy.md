---
title: Security and Privacy Rules
description: What must never be logged and how to keep logs safe for multi-tenant environments.
categories: [platform, logging]
tags: [security, privacy]
weight: 120
---

_Last updated: 24 January 2026_

## Do not log secrets

Never log:
- API keys
- tokens (access/refresh)
- passwords
- card numbers or full payment instrument identifiers
- raw PNR/EMD/SSR details if they contain personal data beyond what you have approved

If you need to reference a credential, log its **record id** (e.g. `EntId=cred_123`) not the secret value.

## Personal data minimisation

Logs should be usable by:
- internal operations
- tenant admins
- vendors/TMCs (where enabled)

Therefore:
- prefer stable ids (`UID`, `ORG`) over names/emails
- do not log passenger names unless explicitly required and approved
- avoid writing full payloads into `message` or `note`

## Multi-tenant safety

- Every tenant-scoped entry must include `TID` and `ORG`.
- Any export or UI viewer must enforce tenant filtering by these fields.
