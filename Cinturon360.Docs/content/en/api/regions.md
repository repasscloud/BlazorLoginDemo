---
title: Regional API Endpoints
---

Cinturon360 operates region-specific API endpoints to support data residency, latency, and regulatory requirements.

Clients **MUST** use the endpoint assigned to their region.

## Available Regions

```
https://au.api.cinturon360.com
https://eu.api.cinturon360.com
https://us.api.cinturon360.com
```

The root domain (`https://api.cinturon360.com`) is not guaranteed to serve production traffic and must not be used for live integrations.

## Regional Assignment

- Region assignment is determined during tenant provisioning.
- Cross-region access is not supported unless explicitly contracted.
- All authentication tokens are region-bound.
