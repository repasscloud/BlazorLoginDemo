---
title: Duffel
description: Duffel booking, approval, and settlement flows
categories: [duffel, external-api]
tags: [duffel, external-api]
weight: 3
---

## My Diag

```mermaid
sequenceDiagram

participant U as 👤 User
participant W as ☁️ Cinturon360

participant E as 🔌 External API
participant Org as 👥 Client

    U->>W: Create booking request
    W-->>W: Evaluate Org Policy, TMC rules
    note over W: QuoteID generated
    
    W-->>U: Search page returned
    U->>W: Submit search request
    W-->>U: Show loading screen

    note over W: QuoteID Config START
    W-->>E: POST req to external API
    E-->>W: Data returned
    note over W: QuoteID Config END

    W-->>U: Results displayed
    U->>W: Submit for approval

    note over W: START Approval
    alt Org Approval Required
        W-->>Org: Request approval
        Org-->>W: Approval granted
    else Auto Approve
        W-->>W: Auto approval granted
    end
    note over W: END Approval

    W->>E: Submit Order
    E->>W: PNR returned

    W-->>U: Approval screen
    note over U: User exit

    W->>E: Submit payment requests
    W->>Org: Notify new order
```


    <!--
    participant Q as 🔁 Message Queue
    participant N as 🔔 Notification Service
    participant St as 💳 Stripe
    participant Tmc as 🏢 TMC
    participant S as ⚙️ Background Worker
    -->
