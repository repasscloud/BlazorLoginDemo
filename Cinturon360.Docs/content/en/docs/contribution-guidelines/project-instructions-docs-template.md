# Project Instructions — Documentation Page Template

This project uses a **single canonical Markdown template** for all documentation pages under:

```
content/en/docs/
```

The template file is provided in this repository as:

```
content/en/docs/_templates/page-template.md
```

All new documentation pages **MUST** be written using this template as the starting point.

## Scope

These instructions apply to:

- All documentation pages under `content/en/docs/`
- All sub-sections (billing, payments, logging, compose, html-ui, onboarding, platform, etc.)

These instructions do **not** apply to:

- API documentation
- Release notes
- News, blog posts, or announcements

## Mandatory Structure Rules

All documentation pages **MUST** adhere to the following structural rules.

### Front Matter

Each page **MUST** include YAML front matter at the top of the file:

- `title`
- `description`
- `categories`
- `tags` (if applicable)
- `weight` (only where menu ordering is required)

No additional metadata fields are permitted unless explicitly approved.

### Headings

- Exactly **one** H1 (`#`) heading is permitted
- The H1 **MUST** match the `title` field
- All content **MUST** be placed under H2 (`##`) or lower
- No content is permitted before the first H2 section

### Section Separators

- Horizontal rules (`---`) **MUST NOT** be used to separate content
- Headings alone define structure and hierarchy

### Tone and Language

Documentation uses **authoritative, declarative language**.

- Use: MUST / MUST NOT / REQUIRED / NOT PERMITTED
- Avoid advisory or speculative language
- Avoid marketing or promotional phrasing

## Optional Sections

The following sections are optional and should only be included when relevant:

- Configuration
- Examples
- Edge Cases and Exceptions
- Operational Notes
- See Also

Empty or placeholder sections are **NOT** permitted.

## Diagrams

- Mermaid diagrams may be used where they add clarity
- Diagrams are optional and not required by the template
- Diagrams must support the surrounding text, not replace it

## Cross-Referencing

Where relevant, pages **SHOULD** include a **See Also** section linking to related documentation.

- Links must be relative
- Only include links that materially help the reader

## Source of Truth

The `page-template.md` file is the **source of truth** for documentation structure.

If there is a conflict between an existing page and the template:

- The template wins for all new pages
- Existing pages may be updated incrementally

## Enforcement

These rules are expected to be enforced via:

- Code review
- Automated linting (where configured)

Failure to follow this template may result in documentation changes being rejected.

