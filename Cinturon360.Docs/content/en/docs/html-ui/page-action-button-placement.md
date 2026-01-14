---
title: Page Action Button Placement Standard
description: Defines the required placement and structure of primary and secondary page actions
---

This document defines the **mandatory standard for page-level action buttons** in `Cinturon360.Web` Razor pages.

Action buttons such as **Save**, **Cancel**, and similar page-scoped operations **MUST NOT** be placed at the bottom of the page.

All page actions **MUST** be placed in the **top page header** and rendered **inside the `<EditForm>` component**.

## Disallowed Pattern (Legacy)

The following pattern is **NOT PERMITTED** and must not be used in new or refactored pages:

```html
<div class="mt-3 d-flex gap-2 mb-3">
    <button class="btn btn-primary" type="submit" disabled="@_isSaving">Save</button>
    <button class="btn btn-outline-secondary" type="button" @onclick="Cancel" disabled="@_isSaving">Cancel</button>
</div>
```

### Why this is forbidden

- Actions are visually disconnected from the page context
- Users must scroll to discover primary actions
- Inconsistent with modern application layout standards
- Encourages duplicated button blocks across pages

Any existing usage of this pattern is considered **legacy** and should be migrated when the page is next modified.

## Required Standard

All page actions **MUST** be rendered in the page header using the following structure.

### Canonical Pattern

```html
<EditForm Model="_vm" OnValidSubmit="SaveAsync">
    <DataAnnotationsValidator />

    <div class="d-flex align-items-center justify-content-between mb-3">
        <h2 class="mb-0">Edit License Agreement</h2>

        <div class="btn-group">
            <button class="btn btn-outline-secondary"
                    type="button"
                    @onclick="Cancel"
                    disabled="@_isSaving">
                Cancel
            </button>

            <button type="submit"
                    class="btn btn-primary"
                    disabled="@_isSaving">
                @if (_isSaving)
                {
                    <span class="spinner-border spinner-border-sm me-2"></span>
                }
                Save
            </button>
        </div>
    </div>

    <!-- Page form content continues here -->
</EditForm>
```

## Mandatory Rules

- Page actions **MUST** appear in the top header row
- Actions **MUST** be inside the `<EditForm>` scope
- Only one `btn-primary` action is permitted per page
- Secondary actions **MUST** use outline styles
- Buttons **MUST NOT** be duplicated at the bottom of the page
- Saving state **MUST** disable all actions
- Loading indicators **MUST** appear inside the primary action button

## Structural Intent

This standard ensures:

- Immediate visibility of primary actions
- Consistent page scanning behaviour
- Reduced scroll dependency
- Clear action hierarchy
- Uniform interaction patterns across the application

Any deviation from this structure is not permitted unless explicitly documented elsewhere.
