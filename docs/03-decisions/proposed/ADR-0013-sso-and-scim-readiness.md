# ADR-0013: SSO and SCIM Readiness — First Target and Scope

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

`Cinturon360.Integrations/IdentityProviders/{Apple,Facebook,Google,Microsoft,Oidc,Saml,Scim}` folders are all empty scaffolds. The following packages are referenced but not wired:

- `ITfoxtec.Identity.Saml2 4.17.0`
- `OpenIddict 5.4.0` (only EF Core tables are created via `UseOpenIddict()`)

No external authentication handler is registered in `Cinturon360.Api/Program.cs`. The `UserAuthMethod` entity supports `AuthMethod` values including `Google`, `Microsoft`, `OIDC`, `SAML`, but no handler exists to populate them.

`QUESTIONS.md Q6` addressed external IdP priority but the answer is not captured in code or an ADR. The `LoginCommandHandler` does not branch on `UserAuthMethod` type — it only handles `Password` authentication.

SCIM (System for Cross-domain Identity Management) is important for enterprise customers who want to provision/deprovision users from their corporate IdP (Okta, Azure AD, etc.) without manual admin work in Cinturon360.

## Decision

*Not yet decided.* The decision must specify:

1. **First SSO target:** Which external IdP is implemented first?
   - Recommended: **Microsoft Entra ID (OIDC)** — the most common corporate IdP for the Cinturon360 target market (travel management for corporate clients). This reuses OpenIddict's OIDC client support (already installed).
   - Alternative: Google Workspace (common for SMBs), or generic OIDC (broadest coverage).

2. **SAML scope:** SAML 2.0 (`ITfoxtec.Identity.Saml2`) is required for enterprise customers with older IdPs (Ping Identity, ADFS). Should SAML be scoped for v5.1, v5.2, or a future major version?
   - Recommended: Defer SAML to v5.2; build generic OIDC first (wider coverage with less complexity).

3. **SCIM scope:** SCIM 2.0 provides automated user and group provisioning from an enterprise IdP. Define the v5.1 scope:
   - Recommended: Spec the SCIM endpoint contract and data model in v5.1; implement the SCIM provisioner in v5.2 with Microsoft Entra as the first SCIM client.

4. **MFA integration:** External SSO sessions imply the IdP handles MFA. The Cinturon360 `IsMfaEnabled` / `UserMfaMethod` model must be bypassed for SSO-authenticated users. Define the bypass rule.

## Consequences

**If Microsoft Entra ID (OIDC) is chosen as the first target:**
- `OpenIddict.AspNetCore` must be wired in `Program.cs` (`AddOpenIddict()` + `AddServer()` or `AddClient()`).
- The `Cinturon360.Integrations/IdentityProviders/Oidc/` folder must be populated with a client implementation.
- The `LoginCommandHandler` (or a new `OidcLoginCommandHandler`) must handle the OIDC callback, look up or create a `User` + `UserAuthMethod.Oidc`, and issue the internal JWT.
- `UserAuthMethod.TenantIdpDomain` (or similar) must link an org to its configured IdP so that SSO is org-scoped, not global.

**If SAML is deferred:**
- Remove `ITfoxtec.Identity.Saml2` from `Directory.Packages.props` until SAML is scoped (reduces unused dependency footprint).

**SCIM:**
- Define the SCIM endpoints: `GET/POST /scim/v2/Users`, `GET/PUT/PATCH/DELETE /scim/v2/Users/{id}`, `GET/POST /scim/v2/Groups`.
- The SCIM provisioner must translate SCIM user attributes to `User.Create(...)` + `UserAuthMethod` creation.

**Required follow-up:**
- Populate `QUESTIONS.md Q6` resolution in this ADR once decided.
- Wire OpenIddict OIDC client in `Program.cs`.
- Design the org-level IdP configuration model (`OrgIdpConfiguration` entity or an extension to `Organisation`).
- Define MFA bypass rule for SSO users.
