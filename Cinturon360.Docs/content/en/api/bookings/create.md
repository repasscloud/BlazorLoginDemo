---
title: Create booking
weight: 10
---

## Endpoint

`POST /api/v1/bookings`

Creates a new draft booking for a client.

## Authentication

- Bearer token required
- Scope: `bookings:write`

## Request

### Headers

```
Authorization: Bearer {token}
Content-Type: application/json
```

### Body

```json
{
  "clientId": "client_123",
  "origin": "SYD",
  "destination": "LHR",
  "travelDate": "2026-06-01"
}
```

## Response

### 201 Created

```json
{
  "bookingId": "bk_456",
  "status": "Draft"
}
```

## Errors

| Code | Meaning |
|-----|--------|
| 400 | Invalid request |
| 401 | Not authenticated |
| 403 | Not authorised |
