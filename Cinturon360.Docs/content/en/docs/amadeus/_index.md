---
title: Amadeus API
description: Overview of details in Amadeus API
categories: [amadeus, external-api]
tags: [test, docs]
weight: 2
---

## Amenity Type

| AmenityType | What it refers to in Amadeus flight offers | Typical examples / notes |
| --- | --- | --- |
| `BAGGAGE` | Checked baggage / carry-on allowance and/or baggage-related ancillaries tied to the fare/segment. | Included checked bag(s), extra checked bag purchase, overweight/oversize bag options (availability varies by airline/content). |
| `PRE_RESERVED_SEAT` | Seat selection (pre-assigning a seat before check-in), whether included or purchasable. | Standard seat selection, preferred seat, extra-legroom seat (often branded by airline). |
| `MEAL` | In-flight meal / catering-related inclusions or paid options. | Included meal, buy-on-board meal eligibility, special meal options (content varies). |
| `BRANDED_FARE` | Fare family / branded fare attributes associated with the offer (what’s “in the bundle”). | “Saver / Flex / Business Flex” style bundles; commonly influences refund/change rules, baggage, seat, lounge, etc. Not always a separately purchasable add-on—often descriptive of the fare. |
| `ENTERTAINMENT` | Onboard entertainment availability/inclusion. | Seatback IFE, streaming to device, Wi-Fi entertainment portals (often informational rather than priced). |
| `TRAVEL_SERVICES` | Non-core “journey services” attached to the flight experience (beyond seat/meal/bags). | Priority boarding, fast track, priority check-in, meet-and-assist style services (exact mapping depends on airline/provider). |
| `LOUNGE` | Lounge access inclusion or purchasable lounge access associated with the fare/segment. | Airline lounge invitation included with premium fares, paid lounge pass eligibility. |
| `UPGRADES` | Cabin/class upgrade-related options or entitlements connected to the offer. | Paid upgrade offers, bid/upgrade eligibility, instant upgrade availability (varies heavily by airline/content). |

## Amenity Descriptions

These “amenity descriptions” are free-text labels that typically appear under `amenities[].description` in flight-offer fare details. They are not a stable enum; treat them as display strings and map them into your own normalized categories where needed.

Legend for **Confidence**:
- **High** = clear meaning from wording
- **Medium** = likely meaning, but airline/program-specific nuance possible
- **Low** = ambiguous without carrier/fare context

| Description | Likely meaning | Suggested normalized bucket | Confidence |
|---|---|---|---|
| 100 PERCENT KF MILES EARNED | Earn 100% of base miles in KrisFlyer (KF) program for this fare. | Loyalty / mileage accrual | Medium |
| 125 PERCENT KF MILES EARNED | Earn 125% of base miles in KrisFlyer for this fare. | Loyalty / mileage accrual | Medium |
| 1PC MAX 15LB 7KG 115LCM | Allowance for 1 piece, max 15lb/7kg and total dimensions 115cm. Likely cabin baggage. | Baggage | Medium |
| 3 HOURS LOUNGE ACCESS | Lounge access provided for up to 3 hours. | Lounge | High |
| 30 PERCENT MILES EARNED | Earn 30% of base miles for this fare. Program unspecified. | Loyalty / mileage accrual | Medium |
| 40KG BAGGAGE ALLOWANCE | Total baggage allowance of 40kg (usually checked). | Baggage | High |
| 50 PCT QMILES ACCUMULATION | Earn 50% Qmiles (Qatar Privilege Club). | Loyalty / mileage accrual | Medium |
| 50KG BAGGAGE ALLOWANCE | Total baggage allowance of 50kg (usually checked). | Baggage | High |
| 70 PERCENT MILES EARNED | Earn 70% of base miles for this fare. | Loyalty / mileage accrual | Medium |
| BASIC SEAT | Basic seating product (standard seat, limited perks). | Seat | Medium |
| BEVERAGE | Beverage included and/or available (often “included” but may be informational). | Meal / beverage | Medium |
| BOOKING CHANGE | Booking changes permitted (rules apply). | Change / flexibility | High |
| CABIN BAG 1 PIECE 7 KG | Cabin baggage allowance: 1 piece up to 7kg. | Baggage | High |
| CABIN BAG 7KG X 1 | Cabin baggage allowance: 1×7kg. | Baggage | High |
| CANCELLATION | Cancellation permitted/available (rules apply). | Cancellation / refund | Medium |
| CARRY ON HAND BAGGAGE | Carry-on/hand baggage permitted/included. | Baggage | High |
| CHANGE AFTER DEPARTURE | Changes allowed after departure (usually with restrictions/fees). | Change / flexibility | High |
| CHANGE BEFORE DEPARTURE | Changes allowed before departure (usually with restrictions/fees). | Change / flexibility | High |
| CHANGE FEE | A fee applies when changing booking (amount depends on rules). | Change / flexibility | High |
| CHANGEABLE TICKET | Ticket can be changed (subject to fare rules). | Change / flexibility | High |
| CHECKED BAG | Checked baggage included and/or available. | Baggage | High |
| CHECKED BAG 1 PC 23 KG EACH | Checked baggage allowance: 1 piece at 23kg each. | Baggage | High |
| CHILD DISCOUNT | Discounted pricing/conditions for children. | Pricing rule / passenger type | Medium |
| COMPLIMENTARY BEVERAGES | Complimentary beverages included. | Meal / beverage | High |
| INFANT DISCOUNT | Discounted pricing/conditions for infants. | Pricing rule / passenger type | Medium |
| INFLIGHT WIFI | In-flight Wi-Fi included and/or available. | Connectivity | Medium |
| MEAL | Meal included and/or available. | Meal / beverage | High |
| MEAL BEVERAGE | Meal + beverage included/available. | Meal / beverage | High |
| MEAL OR SNACK | Either meal or snack included/available depending on flight length/service. | Meal / beverage | Medium |
| MEAL VOUCHER | Voucher provided for meals (often used with LCCs/airport food). | Meal / beverage | Medium |
| MILEAGE ACCRUAL | Miles earn applies (generic statement; details elsewhere). | Loyalty / mileage accrual | Medium |
| NO SHOW | No-show policy applies (penalty/forfeit conditions). | No-show / penalties | Medium |
| PRE PAID BAGGAGE | Baggage pre-paid (ancillary included/selected upfront). | Baggage | Medium |
| PRE RESERVED SEAT ASSIGNMENT | Seat can be assigned in advance (pre-reserved). | Seat | High |
| PRE SEAT ASSIGNMENT | Advance seat assignment included/available. | Seat | High |
| PREMIUM SEAT | Premium seat product (preferred/extra legroom/zone seat). | Seat | Medium |
| REFUND BEFORE DEPARTURE | Refund permitted before departure (rules/fees apply). | Refund / cancellation | High |
| REFUNDABLE TICKET | Ticket is refundable (rules apply; may be partial). | Refund / cancellation | High |
| SEAT ASSIGNMENT | Seat assignment is included/available. | Seat | High |
| SEAT SELECTION FORWARD ZONE | Seat selection allowed in forward section/zone. | Seat | Medium |
| SEAT SELECTION STANDARD ZONE | Standard zone seat selection allowed. | Seat | Medium |
| SNACK | Snack included/available. | Meal / beverage | Medium |
| STANDARD SEATING | Standard seating product included. | Seat | High |
| UPGRADE ELIGIBILITY | Eligible for upgrades (paid, points, bid, etc. depends on carrier). | Upgrades | Medium |
| UPGRADE WITH MILES PWM | Upgrade possible using miles (program-specific; “PWM” unclear without context). | Upgrades / loyalty | Low |
| USB POWER | USB power available at seat. | Onboard amenities (power) | High |

### Implementation notes

- Treat `amenities[].description` as a **display string**, not a reliable enum.
- Normalize into your own buckets (Baggage, Seat, Meal, Lounge, Changes, Refunds, Loyalty, Connectivity, Power, Penalties) using keyword rules.
- Keep the raw string for audit/debugging and future mapping improvements.
