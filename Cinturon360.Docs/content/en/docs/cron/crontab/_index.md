---
title: Crontab
description: Cron schedules grouped exactly as defined in the crontab file
weight: 30
---

_Last updated: 24 January 2026_

This page mirrors the **actual structure of the crontab file**.
Jobs are grouped and ordered **exactly as they appear**, so operators can mentally map:

> crontab → documentation → deeper explanations

Each group links to more detailed pages where required.

All schedules run in **UTC**.

## Hourly jobs

### Expiry processing

**Schedule**
```
0 * * * *
```

**Command**
```
curl http://api:8080/v1/cron/expire/hourly
```

**Summary**
Triggers time-based expiry and cleanup logic.

➡ See: [Expiry job details](expiry.md)

### External API connectivity check (Amadeus – test)

**Schedule**
```
5 * * * *
```

**Command**
```
curl http://localhost:8090/v1/cron/extapi-check/amadeus-test
```

**Summary**
Validates outbound connectivity to the Amadeus test environment.

➡ See: [External API check – Amadeus](extapi-amadeus.md)

## Weekly jobs

### Airline master data ingest

**Schedule**
```
0 2 * * 7
```

**Command**
```
curl http://api:8080/api/v1/admin/ingress/airlines-data
```

**Summary**
Refreshes airline reference data on a weekly cadence.

➡ See: [Airline data ingest](airlines-ingest.md)

## Heartbeat & monitoring

### Cron heartbeat

**Schedule**
```
*/5 * * * *
```

**Command**
```
echo "heartbeat $(date -Iseconds)"
```

**Summary**
Writes a heartbeat timestamp used by container health checks.

➡ See also: [Healthcheck]({{< relref "../healthcheck/_index.md" >}})

## Disabled / debug jobs

### High-frequency heartbeat (disabled)

**Schedule**
```
*/30 * * * * *
```

**Summary**
Optional debug-only heartbeat for diagnosing scheduler latency.

Not enabled in production.
