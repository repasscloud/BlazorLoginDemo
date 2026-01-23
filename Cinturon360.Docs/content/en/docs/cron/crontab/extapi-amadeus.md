---
title: External API check – Amadeus (test)
description: Hourly outbound connectivity verification
weight: 20
---

## Purpose

This job validates outbound network connectivity to the Amadeus **test** API.

It exists to detect:
- ISP routing failures
- Firewall regressions
- TLS handshake problems

## Why cron?

This check must run **independently of application traffic** to provide early warning.
