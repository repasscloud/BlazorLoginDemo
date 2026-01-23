---
title: GUID Specifiers
description: See your project in action!
date: 2017-01-05
weight: 4
---

_Last updated: 24 January 2026_

`"N"` is teh **Guid format specifier** meaning:

- 32 digits
- no separators (no hyphens)
- lowercase hex by default in .NET

```csharp
var g = Guid.Parse("d85b1407-351d-4694-9392-03acc5870eb1");

g.ToString("N"); // "d85b1407351d4694939203acc5870eb1"
g.ToString("D"); // "d85b1407-351d-4694-9392-03acc5870eb1"
g.ToString("B"); // "{d85b1407-351d-4694-9392-03acc5870eb1}"
g.ToString("P"); // "(d85b1407-351d-4694-9392-03acc5870eb1)"
g.ToString("X"); // "{0xd85b1407,0x351d,0x4694,{0x93,0x92,0x03,0xac,0xc5,0x87,0x0e,0xb1}}"
```

So `Guid.NewGuid().ToString("N")` gives you a compact, URL/file-name-friendly GUID string like: `e2c56b1f9a6a4c7c9cba1c2d3e4f5678`.