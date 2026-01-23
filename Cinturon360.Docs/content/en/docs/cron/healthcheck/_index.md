---
title: healthcheck.sh
description: Cron container health verification script
weight: 40
---

_Last updated: 24 January 2026_

## Purpose

`healthcheck.sh` is executed by Docker to determine whether the cron
container is **healthy and operational**.

It validates that:
- The cron daemon is running
- The scheduler loop is alive
- Jobs are still executing periodically

This script is referenced by the container `HEALTHCHECK` directive and is
**not** invoked manually.

## Script

```sh
#!/bin/sh
pgrep -x crond >/dev/null 2>&1 || exit 2

[ -f /tmp/cron.heartbeat ] || exit 3
age=$(( $(date +%s) - $(cat /tmp/cron.heartbeat) ))
[ "$age" -le 600 ] || exit 4

if [ -f /tmp/cron.last ]; then
  ageh=$(( $(date +%s) - $(cat /tmp/cron.last) ))
  [ "$ageh" -le 4500 ] || exit 5
fi

exit 0
```

## Exit codes

| Code | Meaning |
|-----:|--------|
| 0 | Healthy |
| 2 | `crond` not running |
| 3 | Heartbeat file missing |
| 4 | Heartbeat stale (older than 10 minutes) |
| 5 | No recent job execution detected |

## Design notes

- The script is intentionally POSIX-compatible (`/bin/sh`)
- No external dependencies are required
- Failure conditions are explicit and machine-readable
- All logic is defensive and side-effect free
