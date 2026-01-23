---
title: Action Button Standard
description: Save, cancel, and secondary action patterns
---

_Last updated: 24 January 2026_

Primary actions are grouped and aligned consistently.

## Canonical Pattern

```html
<div class="d-flex justify-content-end gap-2 mt-4">
    <button type="submit" class="btn btn-primary">Save</button>
    <button type="button" class="btn btn-outline-secondary" @onclick="Cancel">
        Cancel
    </button>
</div>
```

## Rules

- Primary action is always right-aligned
- `btn-primary` used once per page
- Secondary actions are outline style

## Purpose

- Clear action hierarchy
- Muscle-memory consistency
- Reduced accidental submits
