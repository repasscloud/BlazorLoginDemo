---
title: Examples
description: Examples for job and workflow logs, including step breadcrumbs.
categories: [platform, logging]
tags: [jobs, workflows, examples]
weight: 10
---

_Last updated: 24 January 2026_

## Job lifecycle example

```
EVT=AUTO_JOB_ENQUEUED CAT=AUTO ACT=EXEC OUT=OK JOB=flight-search TRY=1 RID=… TID=… UID=… ORG=… NOTE=quoteId=q_123
EVT=AUTO_JOB_START   CAT=AUTO ACT=EXEC OUT=OK JOB=flight-search TRY=1 RID=… DUR=0MS
EVT=INT_CALL_START   CAT=INT  ACT=EXEC OUT=OK PROV=Amadeus RID=…
EVT=INT_CALL_END     CAT=INT  ACT=EXEC OUT=OK PROV=Amadeus STAT=200 DUR=321MS RID=…
EVT=AUTO_JOB_END     CAT=AUTO ACT=EXEC OUT=OK JOB=flight-search TRY=1 RID=… DUR=9134MS CNT=42
```

## Workflow step breadcrumbs

```
EVT=WF_STEP_START CAT=WF ACT=STEP OUT=OK STEP=Validate->Price ENT=Quote EntId=q_991 RID=…
EVT=WF_STEP_END   CAT=WF ACT=STEP OUT=OK STEP=Validate->Price ENT=Quote EntId=q_991 RID=… DUR=212MS
EVT=WF_STEP_START CAT=WF ACT=STEP OUT=OK STEP=Hold->Ticket   ENT=Quote EntId=q_991 RID=…
EVT=WF_ERR        CAT=WF ACT=STEP OUT=ERR STEP=Hold->Ticket  ENT=Quote EntId=q_991 RID=… NOTE=fare_expired
```
