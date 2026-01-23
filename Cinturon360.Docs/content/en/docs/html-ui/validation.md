---
title: Validation Standard
description: Validation rules and display conventions
---

Validation is enforced using **DataAnnotations** and rendered inline.

## Required Markup

```html
<DataAnnotationsValidator />
<ValidationMessage For="() => _vm.Amount" />
```

## Rules

- All forms **MUST** include `DataAnnotationsValidator`
- Validation messages are inline per-field
- No summary-level validation blocks

## Purpose

- Immediate feedback
- Field-level correction
- Reduced cognitive load
