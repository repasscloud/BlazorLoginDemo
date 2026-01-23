---
title: Razor Page Header Standard
description: Standard page header layout used across all Cinturon360.Web Razor pages
---

This document defines the **mandatory page header pattern** used across **all `.razor` pages** in **Cinturon360.Web**.

The header establishes a consistent visual anchor for every page and prevents ad-hoc spacing or layout drift.

## Canonical Header Markup

```html
<div class="d-flex align-items-center justify-content-between mb-3">
    <h2 class="mb-0">Create Discount</h2>
</div>
```

## Rules

- The header **MUST** be the first visual element in the page body
- `<h2>` is required for primary page titles
- No inline styles are permitted
- Spacing is controlled exclusively via Bootstrap utility classes

## Purpose

- Predictable layout across the platform
- Stable insertion point for future page-level actions
- Consistent typography hierarchy
