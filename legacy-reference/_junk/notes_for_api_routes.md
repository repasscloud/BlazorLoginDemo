
* Marketing site and API share more edge/security configuration
* Harder to apply different limits/controls cleanly later

Pragmatic pick: **`api.cinturon360.com` + `/v1`**. Don’t add `/api` there; it’s redundant.

---

## 2) Enterprise-ready API layout (travel platform: flights + hotel + car + rail + eSIM + approvals + bookings + PNR)

### High-level principles

* Use **bounded contexts** as top-level groupings.
* Use **resources** for state (“search”, “quote”, “booking”, “pnr”, “trip”).
* Use **commands** only where a pure resource update is awkward (`:cancel`, `:issue`, `:refund`, etc.).
* Standardize paging/filtering, errors, idempotency, and async jobs.

---

## 3) Versioned route map (starter layout)

### Core / Platform

* `GET  /v1/meta` (build, ver## 1) Base URL: `/api` path vs `api.` subdomain

Either works. Pick based on how you’ll deploy and who consumes it.

### Recommended for “enterprise + multiple clients”: **`https://api.cinturon360.com/v1/...`**

Pros

* Clean separation of concerns (API vs marketing site vs web app)
* Easier WAF/rate limits/headers/caching policies dedicated to API
* Cleaner CORS story (Web app on `app.` calling `api.`)
* Lets you keep `cinturon360.com` for product/marketing and `app.cinturon360.com` for UI

Cons

* You’ll handle cookies less often (but APIs should be token-based anyway)

### When `/api` on the same host is fine: **`https://cinturon360.com/api/v1/...`**

Pros

* Simpler DNS/infra if you’re small
* Same-origin is convenient if you rely on cookies

Cons
sion, features)
* `GET  /v1/health`
* `GET  /v1/health/ready`
* `GET  /v1/health/live`

### Auth / Identity

* `POST /v1/auth/login`
* `POST /v1/auth/refresh`
* `POST /v1/auth/logout`
* `GET  /v1/me`
* `GET  /v1/me/scopes`

### Orgs / Tenancy / RBAC

(If org is implicit from token, use `/org`; if you need admin cross-org, add `/orgs/{orgId}` endpoints later.)

* `GET  /v1/org`
* `PATCH /v1/org`
* `GET  /v1/org/users`
* `POST /v1/org/users` (create user)
* `POST /v1/org/users/invites`
* `GET  /v1/org/roles`
* `PUT  /v1/org/roles/{roleId}`

### Travellers / Profiles (people who travel)

* `GET  /v1/travellers`
* `POST /v1/travellers`
* `GET  /v1/travellers/{travellerId}`
* `PATCH /v1/travellers/{travellerId}`
* `GET  /v1/travellers/{travellerId}/documents` (passport/visa metadata, not raw docs unless needed)

### Policies (travel policy, approval policy)

* `GET  /v1/policies/travel`
* `POST /v1/policies/travel`
* `GET  /v1/policies/travel/{policyId}`
* `PUT  /v1/policies/travel/{policyId}`
* `DELETE /v1/policies/travel/{policyId}`
* `POST /v1/policies/travel/{policyId}:set-default`

You can mirror that for approval policies:

* `GET /v1/policies/approvals`
* `POST /v1/policies/approvals/{policyId}:set-default`

---

## 4) Searches (multiple search types) — recommended pattern

You have two clean options:

### Option A (recommended): **separate resources per search type**

Keeps DTOs and result shapes clean and strongly typed.

* `POST /v1/flight-searches`
* `GET  /v1/flight-searches/{searchId}`
* `GET  /v1/flight-searches/{searchId}/results`
* `POST /v1/flight-searches/{searchId}:cancel`

Repeat for each:

* `POST /v1/hotel-searches`
* `POST /v1/car-searches`
* `POST /v1/rail-searches`
* `POST /v1/esim-searches` (if “search” means availability/plans by country/device)

### Option B: **single polymorphic search resource**

Good if you have a unified “shopping cart” flow, but it complicates typing.

* `POST /v1/searches` with `{ type: "flight" | "hotel" | "esim", criteria: {...} }`
* `GET /v1/searches/{id}`
* `GET /v1/searches/{id}/results`

For most enterprise codebases, Option A is easier to maintain and document.

---

## 5) Quote / Offer layer (strongly recommended)

Separating “search results” from “purchasable offer/quote” avoids a ton of pain.

### Flights (example)

* `POST /v1/flight-quotes` (from `{ searchId, selectedResultKey, travellers... }`)
* `GET  /v1/flight-quotes/{quoteId}`
* `POST /v1/flight-quotes/{quoteId}:reprice` (optional)

### Hotels / Cars / Rail

* `POST /v1/hotel-quotes`
* `POST /v1/car-quotes`
* `POST /v1/rail-quotes`

### eSIM

Depending on your model:

* `POST /v1/esim-quotes` (plan + quantity + countries + start date)
* `GET  /v1/esim-quotes/{quoteId}`

---

## 6) Bookings / Orders / Trips (the “done” layer)

### Use a top-level “orders” abstraction (especially if you bundle flight + hotel + eSIM)

Enterprise platforms usually end up here.

* `POST /v1/orders` (create from one or more quotes)
* `GET  /v1/orders/{orderId}`
* `GET  /v1/orders/{orderId}/items`
* `POST /v1/orders/{orderId}:cancel` (whole order)
* `POST /v1/orders/{orderId}/items/{itemId}:cancel` (line-level cancel)

If you prefer “bookings”:

* `POST /v1/bookings` (single product booking)
* `GET  /v1/bookings/{bookingId}`
  But “orders” scales better for multi-item itineraries.

### PNRs / Tickets / Itineraries (travel-specific artifacts)

* `GET  /v1/pnrs/{pnrId}`
* `GET  /v1/tickets/{ticketId}`
* `GET  /v1/trips/{tripId}`
* `GET  /v1/trips/{tripId}/itinerary`
* `POST /v1/trips/{tripId}:resync` (pull latest from provider)

### eSIM fulfilment

* `POST /v1/esims` (purchase/provision from quote or order item)
* `GET  /v1/esims/{esimId}` (status, ICCID/EID/activation, QR if applicable)
* `POST /v1/esims/{esimId}:reissue` (if supported)
* `POST /v1/esims/{esimId}:cancel` (if provider allows)

---

## 7) Approvals (travel policy / expense / manager signoff)

* `GET  /v1/approvals`
* `GET  /v1/approvals/{approvalId}`
* `POST /v1/approvals/{approvalId}:approve`
* `POST /v1/approvals/{approvalId}:reject`
* `GET  /v1/approvals/{approvalId}/history`

Link orders/trips to approvals:

* `GET /v1/orders/{orderId}/approvals`

---

## 8) Payments / Invoicing (even if “later”, reserve the namespace)

* `GET  /v1/payment-methods`
* `POST /v1/payment-intents`
* `GET  /v1/invoices`
* `GET  /v1/invoices/{invoiceId}`

---

## 9) Files (receipts, docs, support attachments)

Enterprise pattern: pre-signed upload + metadata record.

* `POST /v1/files/uploads` → `{ uploadUrl, fileId }`
* `POST /v1/files/{fileId}:complete` (optional finalize)
* `GET  /v1/files/{fileId}`
* `DELETE /v1/files/{fileId}`

---

## 10) Webhooks / Events / Audit (enterprise expectation)

### Webhooks

* `POST /v1/webhooks/endpoints`
* `GET  /v1/webhooks/endpoints`
* `DELETE /v1/webhooks/endpoints/{endpointId}`
* `POST /v1/webhooks/endpoints/{endpointId}:rotate-secret`

### Events (optionally let clients poll)

* `GET /v1/events?after=...&types=...`

### Audit log

* `GET /v1/audit-log?actor=...&from=...&to=...`

---

## 11) Admin / Operations (keep separate for safety)

* `GET  /v1/admin/orgs` (if you operate as SaaS admin)
* `POST /v1/admin/orgs`
* `GET  /v1/admin/jobs`
* `GET  /v1/admin/jobs/{jobId}`
* `POST /v1/admin/jobs/{jobId}:retry`

---

## 12) Cross-cutting “enterprise” requirements you should bake in from day 1

### Standard headers

* `X-Correlation-Id` (accept + echo back)
* `Idempotency-Key` (for create/purchase endpoints)
* `Accept-Language` (optional)
* `X-Client` (optional: web/tui/mobile + version)

### Standard response shapes

* lists are always paged: `items + pageInfo`
* errors are always `ProblemDetails` with stable `errorCode` in `extensions`

### Async operations

For search and booking, be comfortable returning:

* `202 Accepted` + `Location: /v1/.../{id}`
* resource has `status: Pending|Running|Completed|Failed|Cancelled`

### Deprecation strategy

* keep `/v1` stable
* introduce `/v2` when truly needed
* optionally support `Sunset` header + `Deprecation` header for old endpoints

---

## 13) Migration approach that won’t hurt

1. Introduce the **new namespaces** in parallel (`/v1/flight-searches`, `/v1/orders`, etc.).
2. Build a thin compatibility layer mapping old endpoints to new handlers.
3. Move clients (Web first, then TUI/Mobile) onto the new SDK interfaces.
4. Deprecate old routes with a published cutoff date + `Deprecation/Sunset` headers.

---

If you want code next: I can generate an **OpenAPI skeleton** (YAML) for these groups and a matching `.NET` typed client interface layout that maps cleanly into `Cinturon360.Shared` (DTOs + `I*Api` interfaces + implementations).

