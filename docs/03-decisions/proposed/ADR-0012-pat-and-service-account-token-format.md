# ADR-0012: PAT and Service Account Token Format

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

`Cinturon360.Infrastructure.Security.TokenService.GeneratePat` issues Personal Access Tokens (PATs) for user API access. The current implementation generates a random 32-byte value, encodes it as base64, and stores a SHA256 hash in the `UserApiToken` table.

The domain model also includes `UserSession.TokenClass` variants: `InteractiveWebSession`, `Mobile`, `API`, etc., covering PATs and service accounts.

There is no documented format for the token string itself — no prefix convention, no length guarantee beyond "32-byte base64", and no rotation policy. This matters because:

- Without a prefix, a leaked token cannot be identified as a Cinturon360 PAT by secret-scanning tools (GitHub Advanced Security, `truffleHog`, etc.).
- Without a defined length, the base64 output length varies if the random source changes.
- Service account tokens are not distinguished from PAT tokens by format alone.
- No rotation mechanism or expiry enforcement is implemented.

The industry best practice (used by GitHub, Stripe, and others) is a structured prefix + encoded random bytes + optional checksum:

- Example: `c360pat_<base62-encoded-random-bytes>` (PAT)
- Example: `c360svc_<base62-encoded-random-bytes>` (service account)

**Evidence:**

- `src/Cinturon360.Infrastructure/Security/TokenService.cs` — `GeneratePat` method
- `src/Cinturon360.Domain/Entities/Identity/UserApiToken.cs` — token entity
- `src/Cinturon360.Domain/Enums/Identity/TokenClass.cs` — `InteractiveWebSession`, `Mobile`, `API`, etc.

## Decision

*Not yet decided.* The decision must specify:

1. **Token prefix convention** — one prefix per token class:
   - Recommended: `c360pat_` (personal access token), `c360svc_` (service account)
2. **Token body format** — recommended: 32 bytes of cryptographically random data, base62-encoded (URL-safe, no padding)
3. **Total token length** — 32 bytes base62 → ~43 chars; with prefix: ~51 chars total
4. **Storage format** — only the SHA256 hash of the raw token is stored (current behaviour); the prefix may or may not be included in the hash input
5. **Hashing algorithm** — SHA256 (current) vs Argon2id/bcrypt. For high-throughput API validation SHA256 is acceptable (the token is already high-entropy); Argon2id is only needed for low-entropy secrets
6. **Expiry and rotation** — PATs should have an optional expiry date (`ExpiresAt` on `UserApiToken`); service account tokens should be rotatable on demand via an admin endpoint
7. **Secret scanning hint** — registering the token prefix with GitHub's secret scanning partner programme (if the repo is public or GitHub Advanced Security is enabled)

## Consequences

**If a prefix + base62 format is adopted:**

- `TokenService.GeneratePat` must be updated to produce `c360pat_<base62(32 random bytes)>`.
- A new `GenerateServiceAccountToken` method must produce `c360svc_<base62(32 random bytes)>`.
- The hash stored in `UserApiToken.TokenHash` must be derived from the full token string including the prefix (so the prefix is part of the secret material).
- Existing PATs issued before this ADR are invalidated (or a migration period must be defined where both formats are accepted).
- A token validation helper must extract the prefix to determine token class before hash lookup.
- Add `ExpiresAt` (nullable) to `UserApiToken` if not already present; enforce expiry in the token validation path.

**Required follow-up:**

- Define the canonical prefix list in `Cinturon360.Common.Constants.TokenPrefixes`.
- Update `TokenService.GeneratePat` and add `GenerateServiceAccountToken`.
- Author a migration for existing tokens if any are in use.
- Register prefixes with GitHub secret scanning if the repository uses GitHub Advanced Security.
- Add token rotation endpoint to the admin API.
