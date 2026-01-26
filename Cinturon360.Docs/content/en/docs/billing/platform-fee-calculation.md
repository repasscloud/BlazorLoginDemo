---
title: Platform Fee Calculation
type: docs
weight: 70
---

## Objective

Given:

- a pass-through airline fee A
- a desired platform margin
- a payment processor fee rate
- a GST rate

Calculate a platform fee P such that:

1. Airline fee is preserved
2. Processor fees are covered
3. Margin is achieved

## Variables

- `A` = airline fee (pass-through)
- `P` = platform fee (ex GST)
- `g` = GST rate (0.10 in AU)
- `r` = processor fee rate
- `k` = desired margin as % of A

## Percentage-based margin formula

`P` is calculated as:

```
P = A × ( k + (1 + g) × r ) / ( 1 − (1 + g)² × r )
```

This formula compensates for:

- processor fees applied to airline fee
- processor fees applied to platform fee
- processor fees applied to GST on platform fee

## Applying GST

Invoice lines:

- Platform fee (ex GST): `P`
- GST on platform fee: `g × P`
- Platform fee (inc GST): `P × (1 + g)`

Total charged:

```
A + P × (1 + g)
```

## Worst-case vs exact margin

If `r` is assumed as worst-case (e.g. AMEX):

- margin is guaranteed minimum
- actual margin may be higher

Exact margin requires card-specific rates.
