---
title: UI Traceability
description: "Standard UI events used to trace user intent: page opens, edit mode, saves, and function execution."
categories: [platform, logging]
tags: [ui, traceability]
weight: 50
---

## What UI traceability logs are for

UI logs capture **user intent** in the interface.
They enable reconstruction of a session without requiring access to browser telemetry.

UI logs are used to answer:
- Which user opened which page, and when?
- Which record/section did they intend to edit?
- Did they press Save?
- Did they execute a privileged function (e.g. “Test Connection”) and from where?

UI logs should be emitted for **deliberate actions** only:
- route navigation
- edit mode toggles
- button actions (save, run, execute)

Do **not** log:
- keystrokes
- input change events
- render cycles
- hover events

## Standard UI events

The platform standardises the following UI events.
Each includes recommended `CAT`, `ACT`, `OUT`, `ENT`, and what to place in `NOTE`.

- [UI_PAGE_OPEN](./ui-page-open/)
- [UI_EDIT_MODE_ENABLED](./ui-edit-mode-enabled/)
- [UI_SAVE_PRESSED](./ui-save-pressed/)
- [UI_FUNCTION_EXECUTED](./ui-function-executed/)
