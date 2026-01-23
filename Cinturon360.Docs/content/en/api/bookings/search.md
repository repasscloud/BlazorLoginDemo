---
title: Search bookings
weight: 20
---

## Endpoint

`GET /api/v1/bookings`

Returns bookings matching the supplied filters.

## Request

### Query parameters

| Name | Type | Required |
|-----|------|----------|
| clientId | string | No |
| status | string | No |

## Response

```json
{
  "items": [
    {
      "bookingId": "bk_456",
      "status": "Draft"
    }
  ]
}
```
