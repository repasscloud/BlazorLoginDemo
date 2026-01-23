---
title: Account Thresholds
description: How maximum spend thresholds influence auto-approval and billing eligibility.
categories: [platform, payments]
tags: [financials, payments]
weight: 30
---

## Fields

- **Account Threshold**: the maximum spend allowed before auto-approval is disabled.
- **Threshold Scope**: when the threshold resets.
  - PAYG
  - Monthly
  - Quarterly
  - Bi-Annual
  - Annual

## Purpose

Thresholds are primarily used to control:

- automatic approvals
- exposure and risk for unattended charging
- travel policy enforcement

## Behaviour

When threshold is exceeded:

- auto-approval MUST be disabled for charges that would exceed the limit
- manual approval flow must be enforced
- reporting must reflect the threshold breach

Threshold resets occur at the start of the next scope interval.
