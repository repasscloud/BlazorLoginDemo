---
title: Standard Integration Events
description: Standard INT_* events and examples for provider calls.
categories: [platform, logging]
tags: [integrations, events]
weight: 10
---

_Last updated: 24 January 2026_

## INT_CALL_START

```
EVT=INT_CALL_START CAT=INT ACT=EXEC OUT=OK PROV=Amadeus RID=… TID=… UID=…
```

## INT_CALL_END

```
EVT=INT_CALL_END CAT=INT ACT=EXEC OUT=OK PROV=Amadeus STAT=200 DUR=321MS RID=…
```

## INT_TIMEOUT

```
EVT=INT_TIMEOUT CAT=INT ACT=EXEC OUT=TIMEOUT PROV=Amadeus DUR=30000MS RID=… NOTE=timeout_budget_exceeded
```

## INT_ERR

```
EVT=INT_ERR CAT=INT ACT=EXEC OUT=ERR PROV=Stripe STAT=402 RID=… NOTE=card_declined
```

## INT_RETRY

Include attempt count as `TRY` if available.

```
EVT=INT_RETRY CAT=INT ACT=EXEC OUT=RETRY PROV=Amadeus TRY=2 RID=… NOTE=transient_error
```
