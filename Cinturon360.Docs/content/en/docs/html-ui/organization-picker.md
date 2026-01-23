---
title: Organization Picker Pattern
description: Async organization selector used in NewDiscount.razor
---

_Last updated: 24 January 2026_

The organization picker uses a **read-only display field** with explicit selection and clear actions.

## Canonical Markup

```html
<InputText class="form-control" Value="_parentOrgDisplay" disabled />
<div class="btn-group">
    <button class="btn btn-outline-primary" type="button" @onclick="PickParentAsync">
        Select
    </button>
    <button class="btn btn-outline-secondary" type="button" @onclick="ClearParent">
        Clear
    </button>
</div>
```

## Rules

- Display field is always disabled
- Selection is explicit (no dropdowns)
- Clear action must exist if selection is optional

## Purpose

- Prevent accidental changes
- Support async modal-based selection
- Clear audit trail in UI behaviour
