---
title: GUID Format Specifiers
description: Standard GUID string formats used within the platform and their characteristics
categories: [platform, identifiers]
tags: [guid, identifiers]
weight: 4
type: docs
---

## Overview

.NET provides multiple **GUID format specifiers** that control how a `Guid` value is rendered as a string.
These formats are often relevant when GUIDs are used as **identifiers**, **keys**, or **external references**.

This page documents the available formats and highlights the recommended usage within the platform.

## The `"N"` format (no separators)

The `"N"` format specifier produces a **compact GUID representation**:

- **32 hexadecimal characters**
- **No separators** (no hyphens, braces, or parentheses)
- Lowercase hexadecimal by default in .NET

This format is particularly suitable for:
- URLs
- filenames
- compact identifiers
- systems where separators are undesirable

## Examples

```csharp
var g = Guid.Parse("d85b1407-351d-4694-9392-03acc5870eb1");

g.ToString("N"); // "d85b1407351d4694939203acc5870eb1"
g.ToString("D"); // "d85b1407-351d-4694-9392-03acc5870eb1"
g.ToString("B"); // "{d85b1407-351d-4694-9392-03acc5870eb1}"
g.ToString("P"); // "(d85b1407-351d-4694-9392-03acc5870eb1)"
g.ToString("X"); // "{0xd85b1407,0x351d,0x4694,{0x93,0x92,0x03,0xac,0xc5,0x87,0x0e,0xb1}}"
```

## Recommended usage

When a GUID must be represented as a **string identifier**, the preferred form is:

```csharp
Guid.NewGuid().ToString("N");
```

Example output:

```
e2c56b1f9a6a4c7c9cba1c2d3e4f5678
```

This format is:
- Compact
- URL- and filename-safe
- Unambiguous
- Widely supported across systems

## Notes

- The `"N"` format is **not case-sensitive** for parsing.
- The underlying GUID value remains unchanged regardless of format.
- Format choice affects **representation only**, not uniqueness or entropy.

## Relationship to platform identifiers

GUIDs formatted with `"N"` are occasionally used for:
- interoperability
- external system references
- legacy integrations

However, **platform-native identifiers** generally prefer:
- explicit prefixes
- NanoID-based random components
- lifecycle-driven length selection

See the rest of the [Identifiers](.././identifiers/types/) section for platform-specific identifier standards.