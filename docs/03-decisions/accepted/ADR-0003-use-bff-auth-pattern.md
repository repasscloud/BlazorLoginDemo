# ADR-0003: Use BFF Auth Pattern (Cookie-Only Browser, JWT in Claims)

**Status:** Accepted  
**Date:** 2026-05-10  
**Deciders:** Cinturon360 engineering team

## Context

`Cinturon360.Web` (Blazor Server) must authenticate against `Cinturon360.Api` (JWT-bearer). The standard approach of storing a JWT in browser `localStorage` or a non-HttpOnly cookie exposes the token to XSS. The Backend-for-Frontend (BFF) pattern keeps tokens server-side and issues only a secure HttpOnly session cookie to the browser.

Evidence of the implementation:
- `src/Cinturon360.Web/Security/BffAuthStateProvider.cs`
- `src/Cinturon360.Web/Security/BffTokenHandler.cs`
- `src/Cinturon360.Web/DependencyInjection/WebServiceRegistration.cs:20–38`
- `src/Cinturon360.Web/Endpoints/AuthEndpoints.cs:15–183`

## Decision

The Web project acts as a BFF. The browser authenticates via a cookie scheme named `c360.bff`; the Web host exchanges this cookie for an API JWT on every outbound API call.

Specifics:
- Cookie: `HttpOnly`, `SameSite=Strict`, `SecurePolicy=SameAsRequest`, sliding 1-day expiry
- On login (`POST /auth/login-handler`): Web calls `IdentityApiClient.LoginAsync`, receives an `AuthResponse`, and writes the access token into the cookie's `Cinturon360.Web.Security.ClaimTypes.AccessToken` claim
- `BffTokenHandler` (a `DelegatingHandler`) reads `AccessToken` from the cookie claims and adds `Authorization: Bearer <jwt>` to all outgoing API `HttpClient` calls
- The browser never sees the raw JWT

Current limitation: the refresh-token claim is stored empty; the refresh flow is not implemented.

## Consequences

**Positive**
- API JWTs are never exposed to browser JavaScript; XSS cannot steal tokens.
- All nine typed API clients (`IdentityApiClient`, `TravellerApiClient`, etc.) benefit automatically via `BffTokenHandler`.
- Compatible with Blazor Server's server-side execution model.

**Negative / Trade-offs**
- The raw JWT is currently stored as a claim value inside the ASP.NET cookie ticket; this means the full token (including all `perm` claims) is persisted in the encrypted cookie. As the `perm` claim list grows, cookie size will approach the 4 KB browser limit.
- Refresh flow is unimplemented: after `AccessTokenExpiryMinutes` (default 30 min), outgoing API calls will receive 401 with no automatic recovery until the user re-logs in.
- `SecurePolicy=SameAsRequest` means the cookie is not forced to HTTPS in development; a staging/production override is needed.

**Required follow-up**
- Implement the refresh-token flow: store a refresh token server-side (keyed by session ID) and renew the access token silently when it expires.
- Consider moving the access token out of the cookie payload into a server-side cache (Redis or EF) keyed by session ID to eliminate cookie bloat.
- Set `SecurePolicy=Always` in non-development environments.
