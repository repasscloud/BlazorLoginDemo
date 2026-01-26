---
title: Worked Examples
type: docs
weight: 80
---

## Example

- Airline fee: 1279.91
- Margin: 5%
- GST: 10%
- Processor rate: 3.5%

Computed platform fee (ex GST): 118.28
GST on platform fee: 11.83

Total charged:
1279.91 + 118.28 + 11.83 = 1410.02

## Stripe breakdown interpretation

Stripe shows:

- Payment amount: 1410.02
- Processing fee
- GST on processing fee
- Net amount

Net amount ≥ airline fee confirms pass-through preservation.
