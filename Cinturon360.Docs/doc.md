---
title: Root Cause Analysis (RCA) / Post Incident Review (PIR) – {Incident_Title}
date: {Publish_Date_YYYY-MM-DD}
description: >
  Client-facing incident report for {Incident_Title}. Includes impact summary,
  timeline, root cause, corrective actions, and prevention measures.
layout: policy
show_description: false
categories: [incidents, rca, pir]
tags: [incident, rca, pir, reliability]
---

<div class="alert alert-info">
<strong>Document type:</strong> Root Cause Analysis (RCA) / Post Incident Review (PIR)<br/>
<strong>Audience:</strong> Clients and stakeholders<br/>
<strong>Incident ID:</strong> {Incident_Id}<br/>
<strong>Status:</strong> {Status_Draft_or_Final}<br/>
<strong>Last updated:</strong> {Last_Updated_YYYY-MM-DD HH:mm TZ}
</div>

## Executive summary

On **{Incident_Date_YYYY-MM-DD}**, Cinturon360 experienced **{High_Level_Incident_Summary}** impacting **{Impacted_Areas}**.
The incident began at **{Start_Time_UTC}** and was fully resolved at **{End_Time_UTC}**.

**Impact:** {One_Sentence_Impact}  
**Root cause:** {One_Sentence_Root_Cause}  
**Resolution:** {One_Sentence_Resolution}  
**Prevention:** {One_Sentence_Prevention_Theme}

---

## Severity and classification

**Severity (select one):**
- [ ] Critical – Widespread outage or severe impact affecting most or all customers
- [ ] High – Significant degradation or functional loss affecting many customers
- [ ] Medium – Partial impact affecting a subset of customers or functionality
- [ ] Low – Minor issue with limited impact or workaround available
- [ ] Informational – No customer impact; transparency or preventative disclosure

**Incident type (select all that apply):**
- [ ] Availability (service down)
- [ ] Degraded performance (latency/timeouts)
- [ ] Data correctness (incorrect results)
- [ ] Data access / security (access controls / exposure)
- [ ] Third-party dependency (provider outage / degradation)
- [ ] Deployment / change-related
- [ ] Capacity / scaling
- [ ] Other: {Other_Incident_Type}

**Customer-facing?**
- [ ] Yes
- [ ] No

**Environments affected (select all that apply):**
- [ ] PROD
- [ ] UAT
- [ ] TEST
- [ ] DEV

**Components affected (select all that apply):**
- [ ] Web application
- [ ] Public API
- [ ] Database / SQL layer
- [ ] Mobile app – Android
- [ ] Mobile app – iOS
- [ ] Desktop application
- [ ] CLI tooling
- [ ] Background workers / queue processing
- [ ] Integrations: {Integration_Names}

---

## Customer impact

### Who was impacted
- **Tenants / clients impacted:** {Client_Scope} (e.g., “subset of tenants”, “all tenants”, “one tenant”)
- **User roles impacted:** {Roles_Impacted} (e.g., Admins, Travellers, Approvers)
- **Geographic scope:** {Geo_Scope} (if relevant)
- **Duration of impact:** {Impact_Duration}

### What was impacted
- **User-visible symptoms:**
  - {Symptom_1}
  - {Symptom_2}
  - {Symptom_3}

- **Functional impact (examples):**
  - {Function_Impacted_1}
  - {Function_Impacted_2}

### What was not impacted (important)
- {Non_Impacted_Area_1}
- {Non_Impacted_Area_2}

### Data impact
- **Data loss occurred:**
  - [ ] Yes
  - [ ] No

- **Data correctness impacted:**
  - [ ] Yes
  - [ ] No

If **Yes** to any of the above:
- **Data types affected:** {Data_Types}
- **Extent:** {Extent}
- **Customer action required:** {Customer_Action_Required_or_None}

---

## Detection and response

### How the incident was detected
- [ ] Automated alerting
- [ ] Internal monitoring dashboards
- [ ] Client report / support ticket
- [ ] Partner / third-party notification
- [ ] Other: {Detection_Other}

**Detection details:** {Detection_Details}

### Time to key milestones
- **Time to detect (TTD):** {TTD}
- **Time to acknowledge (TTA):** {TTA}
- **Time to mitigate (TTM):** {TTM}
- **Time to resolve (TTR):** {TTR}

---

## Timeline (UTC)

> Use UTC for consistency. If you must include local time, include it as secondary.

| Time (UTC) | Event |
|---|---|
| {T0_UTC} | Incident begins / first customer impact observed |
| {T1_UTC} | Alert triggered / issue detected |
| {T2_UTC} | Incident declared; response team engaged |
| {T3_UTC} | Mitigation applied ({Mitigation_Short}) |
| {T4_UTC} | Service stabilised; monitoring confirms recovery |
| {T5_UTC} | Full resolution; follow-up validation complete |
| {T6_UTC} | Client communication sent (initial / update / resolved) |

---

## Root cause analysis

### Summary of root cause
{Root_Cause_Paragraph}

### Contributing factors
- {Contributing_Factor_1}
- {Contributing_Factor_2}
- {Contributing_Factor_3}

### Why it happened (5 Whys)

1. **Why did the customer experience {Symptom}?**  
   {Why_1}

2. **Why did {Why_1} occur?**  
   {Why_2}

3. **Why did {Why_2} occur?**  
   {Why_3}

4. **Why did {Why_3} occur?**  
   {Why_4}

5. **Why did {Why_4} occur?**  
   {Why_5}

**Root cause statement:** {Root_Cause_Statement}

---

## Resolution and recovery

### What we did to mitigate the issue
- {Mitigation_Action_1}
- {Mitigation_Action_2}

### What we did to fully resolve the issue
- {Resolution_Action_1}
- {Resolution_Action_2}

### Verification
We confirmed recovery through:
- [ ] Monitoring returned to baseline
- [ ] Error rates normalised
- [ ] Synthetic checks passed
- [ ] Targeted functional validation completed
- [ ] Customer confirmation (where applicable)

**Verification notes:** {Verification_Notes}

---

## Customer communications

### What we communicated
- **Initial notification (time):** {Initial_Notification_Time_UTC}
- **Update(s) sent (times):** {Update_Times_UTC}
- **Resolution notice (time):** {Resolution_Notice_Time_UTC}

### Where we communicated
- [ ] Email
- [ ] Status page
- [ ] In-app notification
- [ ] Support ticket
- [ ] Other: {Comms_Other}

---

## Corrective and preventive actions (CAPA)

> Actions should be specific, owned, measurable, and time-bound.

| Action | Type | Owner | Target date | Status |
|---|---|---|---|---|
| {Action_1} | Preventative | {Owner_1} | {Date_1} | {Status_1} |
| {Action_2} | Corrective | {Owner_2} | {Date_2} | {Status_2} |
| {Action_3} | Detective | {Owner_3} | {Date_3} | {Status_3} |
| {Action_4} | Process | {Owner_4} | {Date_4} | {Status_4} |

### Follow-up validation plan
- {Validation_Item_1}
- {Validation_Item_2}

---

## What went well

- {Went_Well_1}
- {Went_Well_2}

---

## What didn’t go well

- {Didnt_Go_Well_1}
- {Didnt_Go_Well_2}

---

## Lessons learned

- {Lesson_1}
- {Lesson_2}

---

## Appendix (optional)

### A. Impact metrics (if applicable)
- **Availability (estimated):** {Availability_Percent}
- **Error rate peak:** {Error_Rate}
- **Latency peak:** {Latency}
- **Requests affected:** {Requests_Affected}

### B. Glossary
- **PROD:** Production environment used by clients
- **UAT:** User acceptance testing environment
- **TTD/TTA/TTM/TTR:** Standard incident response timing metrics

---
