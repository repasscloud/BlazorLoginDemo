---
title: Default Platform-Hosted Terms
description: How default legal payment authorization terms are provided and used by Cinturon360.
categories: [platform, payments]
tags: [legal, default]
weight: 20
---

_Last updated: 24 January 2026_

## Purpose

Cinturon360 provides default legal terms to ensure that:

- Clients can authorise stored payment credentials
- off-session charges for approved travel can be initiated lawfully and consistently
- disputes and audits can be supported with consistent evidence

## What “hosted by Cinturon360” means

- The platform ships with a default agreement template (standing payment authorisation).
- The template is versioned and controlled at Vendor scope.
- A rendered copy is generated for each Client authorisation request and stored for audit.

## When default terms are used

Default terms are used when:

- the TMC has not selected a custom uploaded agreement for the Client relationship, or
- the Vendor mandates default terms for certain models

## Evidence requirements

When default terms are used, the platform MUST store:

- template version identifier (or hash)
- rendered copy issued to the Client
- acceptance evidence (who, when, how)
- revocation channel details
- mapping to Client–TMC relationship

## Standing vs as-required language

Default terms are written as a **standing** authorisation that supports **as-required** charging:

- standing authorisation: acceptance persists until revoked
- as-required charging: charges occur only when an approved travel charge arises

This distinction should be reflected in UI copy and onboarding emails.
