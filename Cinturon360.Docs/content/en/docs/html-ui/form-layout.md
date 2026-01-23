---
title: Form Layout Standard
description: Grid-based form layout pattern
---

All data-entry pages use a **Bootstrap grid-based form layout**.

## Canonical Structure

```html
<EditForm Model="_vm" OnValidSubmit="SaveAsync">
    <DataAnnotationsValidator />

    <div class="row g-3">
        <div class="col-12 col-md-6">
            ...
        </div>
        <div class="col-12 col-md-6">
            ...
        </div>
    </div>
</EditForm>
```

## Rules

- Forms **MUST** be wrapped in `<EditForm>`
- `row g-3` is mandatory for spacing
- Columns must collapse to `col-12` on mobile
- No nested rows inside inputs

## Purpose

- Responsive layout by default
- Consistent spacing between fields
- Predictable tab order
