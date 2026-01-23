---
title: Input Field Pattern
description: Standard label + input pattern for Razor forms
---

_Last updated: 24 January 2026_

Each input follows a strict **label + control + validation** pattern.

## Canonical Pattern

```html
<div class="form-group">
    <label class="form-label">Discount Name</label>
    <InputText class="form-control" @bind-Value="_vm.Name" />
    <ValidationMessage For="() => _vm.Name" />
</div>
```

## Rules

- Labels **MUST** be explicit, never placeholders
- `form-control` is required
- Validation messages are always inline
- No helper text unless explicitly required

## Purpose

- Accessibility compliance
- Predictable validation placement
- Consistent visual rhythm
