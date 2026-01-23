---
title: C# Examples
description: Concrete examples for emitting logs for UI actions and API validation in the platform logger.
categories: [platform, logging]
tags: [csharp, examples]
weight: 10
---

_Last updated: 24 January 2026_

## UI: page open

```csharp
await _log.InformationAsync(
    evt: "UI_PAGE_OPEN",
    cat: SysLogCatType.UI,
    act: SysLogActionType.View,
    message: "User opened page.",
    ent: "Page",
    entId: "/companies/xyz",
    rid: GetCorrelationId(),
    tid: tenantId,
    uid: userId,
    org: orgId,
    http: null,
    stat: null,
    path: "/companies/xyz",
    note: "source=menu");
```

## UI: edit mode enabled

```csharp
await _log.InformationAsync(
    evt: "UI_EDIT_MODE_ENABLED",
    cat: SysLogCatType.UI,
    act: SysLogActionType.Update,
    message: "User enabled edit mode.",
    ent: "Company",
    entId: "xyz",
    rid: GetCorrelationId(),
    tid: tenantId,
    uid: userId,
    org: orgId,
    path: "/companies/xyz",
    note: "section=billing");
```

## UI: save pressed

```csharp
await _log.InformationAsync(
    evt: "UI_SAVE_PRESSED",
    cat: SysLogCatType.UI,
    act: SysLogActionType.Update,
    message: "User pressed Save.",
    ent: "Company",
    entId: "xyz",
    rid: GetCorrelationId(),
    tid: tenantId,
    uid: userId,
    org: orgId,
    path: "/companies/xyz",
    note: "section=billing");
```

## UI: function executed

```csharp
await _log.InformationAsync(
    evt: "UI_FUNCTION_EXECUTED",
    cat: SysLogCatType.UI,
    act: SysLogActionType.Exec,
    message: "User executed a function from UI.",
    ent: "Integration",
    entId: "amadeus",
    rid: GetCorrelationId(),
    tid: tenantId,
    uid: userId,
    org: orgId,
    path: "/settings/integrations/amadeus",
    note: "function=TestConnection");
```

## API: body missing validation

```csharp
if (dto is null)
{
    await _log.WarningAsync(
        evt: "API_VALIDATION_FAIL_BODY_MISSING",
        cat: SysLogCatType.Api,
        act: SysLogActionType.Create,
        message: "Body required.",
        rid: GetCorrelationId(),
        tid: tenantId,
        uid: userId,
        org: orgId,
        durMs: 0,
        http: Request.Method,
        stat: StatusCodes.Status400BadRequest,
        path: HttpContext?.Request?.Path.Value,
        note: "errorCode=validation_failed");

    return BadRequest("Body required.");
}
```
