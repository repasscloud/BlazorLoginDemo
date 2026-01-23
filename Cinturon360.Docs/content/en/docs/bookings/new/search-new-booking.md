# Search (New Booking)

The **Search** page is the first functional step in creating a new booking.  
It is responsible for collecting traveller context, trip parameters, and policy-constrained preferences, then submitting a normalized search request to the backend.

This page exists within the **New Booking** flow and always produces a *draft booking search*.

## Navigation context

The Search page is reached via the following path:

```
Travel
→ New Booking
→ Select booking type
→ Search
```

### Booking type selection

Supported booking types include (but are not limited to):

- Flight
- Accommodation
- Taxi
- Train

This document focuses **only on Flight search**.

## Traveller & organisation context

Before reaching the Search page, the user must select *who the booking is for*.  
The available selection scope depends on the user’s effective role.

### Role-based selection rules

| Running as | May select |
|-----------|-----------|
| **Sudo** | Vendor → TMC → Client → Users |
| **Vendor** | TMC → Client → Users |
| **TMC** | Client → Users |
| **Client user** | Self and/or other permitted users |

Once the traveller context is resolved, the flow continues to Search.

## Search inputs

The Search page collects structured trip parameters.  
All fields are validated client-side before submission.

### Core trip details

| Field | Description |
|-----|------------|
| **Trip type** | `OneWay` or `Return` |
| **From** | Origin airport (IATA code) |
| **To** | Destination airport (IATA code) |
| **Departure date** | Outbound travel date |
| **Return date** | Required only for return trips |

### Time preferences (optional)

These fields narrow results but do not hard-filter availability.

| Field | Meaning |
|-----|--------|
| **Earliest departure** | Earliest acceptable departure time |
| **Latest departure** | Latest acceptable departure time |
| **Earliest return** | Earliest acceptable return time |
| **Latest return** | Latest acceptable return time |

### Cabin & policy-driven preferences

| Field | Notes |
|-----|------|
| **Cabin class** | Preferred cabin (e.g. Economy) |
| **Max cabin class** | Upper bound allowed by policy |
| **Airlines** | Allowed airlines (policy-constrained) |
| **Alliances** | Allowed alliances (policy-constrained) |

If a field is restricted by travel policy, the UI enforces the constraint and prevents invalid selections.

## Search submission

When the user presses **Search**, the UI generates a normalized JSON payload representing the request.

### Example payload

```json
{
  "id": "8OFkxoNFHwzwJu_InurCu",
  "tripType": "OneWay",
  "originIataCode": "SYD",
  "destinationIataCode": "LHR",
  "departureDate": "2026-02-05",
  "returnDate": null,
  "departEarliestTime": "",
  "departLatestTime": "",
  "returnEarliestTime": "",
  "returnLatestTime": "",
  "cabinClass": "Economy",
  "maxCabinClass": "Economy",
  "selectedAirlines": ["QF"],
  "alliances": [],
  "tid": "ef226dab-264e-41f1-9d34-84c2633d96fa",
  "uid": "1074291e-f876-4246-84cc-be4b9f2a8c3b",
  "org": null
}
```

### Key fields

| Field | Purpose |
|-----|--------|
| `id` | Draft booking identifier |
| `tripType` | One-way or return |
| `originIataCode` | Origin airport |
| `destinationIataCode` | Destination airport |
| `departureDate` | Outbound date |
| `returnDate` | Null for one-way trips |
| `cabinClass` | Requested cabin |
| `maxCabinClass` | Policy ceiling |
| `selectedAirlines` | Explicit airline filter |
| `alliances` | Alliance filter |
| `tid` | Tenant / transaction correlation ID |
| `uid` | User ID |
| `org` | Explicit org override (if applicable) |

## API interaction

The payload is submitted as a **patch** against the draft booking.

### Endpoint

```
POST /v1/bookings/drafts/{draftId}/flight-searches
```

Where:

```
{draftId} = id
```

Example:

```
/v1/bookings/drafts/8OFkxoNFHwzwJu_InurCu/flight-searches
```

This call:

- Associates the search with the draft booking
- Persists search intent
- Triggers downstream flight availability processing

## Loading & results

After submission:

1. The UI transitions to a **loading state**
2. Flight availability is retrieved asynchronously
3. Results are rendered once the search completes

The Search page itself does not render results; it only initiates the search and hands off control.

## Summary

The Search page:

- Resolves traveller and organisational context
- Collects trip and policy-constrained preferences
- Produces a normalized search payload
- Submits the request against a draft booking
- Transitions to loading, then results

It is the foundation for all downstream pricing, availability, and policy evaluation.
