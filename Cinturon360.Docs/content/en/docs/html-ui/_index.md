---
title: "HTML & Razor UI Design Standards"
weight: 10
---

## Purpose

This section is the **single source of truth** for all HTML and Razor UI design standards used in **Cinturon360.Web**.

All rules defined here are **authoritative** and MUST be followed by every Razor page, component, and layout unless an explicit exception is documented elsewhere.

This section exists to:
- Centralise HTML and Razor design decisions
- Prevent design drift across pages and features
- Provide clear, enforceable UI standards
- Act as a reference for code reviews and future development

## Scope

The standards in this section apply to:

- Razor Pages (`.razor`)
- Layout files (`MainLayout.razor`, shared layouts)
- Partial components used for page structure
- HTML structure emitted by Blazor components
- Bootstrap usage within Razor files

This section does **NOT** cover:
- Application logic
- C# service or domain design
- CSS implementation details (unless tightly coupled to HTML structure)
- JavaScript behaviour

## Folder Structure

All HTML and Razor UI design documentation MUST live under:

```
docs/html-ui/
```

This folder represents **presentation-layer standards** only.

### Rationale

The name `html-ui` is intentional:

- **html** — clearly indicates markup-level concerns
- **ui** — scopes content to user interface rules, not styling frameworks or logic
- Avoids ambiguity with:
  - `razor` (too implementation-specific)
  - `frontend` (too broad)
  - `html-design` (vague and subjective)

## Document Rules

Each document in this section MUST:

1. Describe a **single, focused standard**
2. State **when it MUST be used**
3. Explicitly document:
   - What is allowed
   - What is forbidden
4. Include **correct vs incorrect** examples where applicable
5. Use definitive language:
   - MUST
   - REQUIRED
   - NOT PERMITTED

Opinionated or optional language is NOT allowed unless explicitly stated.

## Example Documents

Typical documents in this section include:

- `razor-page-header-standard.md`
- `form-layout-standard.md`
- `page-toolbar-standard.md`
- `empty-state-standard.md`
- `list-and-table-structure.md`

Each document defines **structure**, not visual styling.

## Existing Content

The existing document:

```
razor-page-header-standard.md
```

MUST be moved under this folder:

```
docs/html-ui/razor-page-header-standard.md
```

No content changes are required unless explicitly refactored later.

## Enforcement

Any Razor UI that does not conform to the standards defined in this section is considered **non-compliant** and MUST be corrected.

Exceptions MUST be documented and approved explicitly.
