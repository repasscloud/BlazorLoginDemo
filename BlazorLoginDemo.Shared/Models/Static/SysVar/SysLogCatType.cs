namespace BlazorLoginDemo.Shared.Models.Static.SysVar
{
    // Stable, extensible numeric buckets. 990–999 reserved for Misc only.
    public enum SysLogCatType : int
    {
        // 100–199 Security & Identity
        Auth            = 110, // sign-in/out, token issuance
        Authorization   = 120, // RBAC/policy decisions
        AccessControl   = 130, // grants/revokes, ACL changes
        Audit           = 140, // compliance trails (read/write)
        Privacy         = 150, // PII handling, redaction, DSRs

        // 300–399 App & UI
        App             = 310, // app lifecycle, feature toggles (UI scope)
        Ui              = 320, // page views, navigation, UX events
        UserAction      = 330, // explicit user intents (create, submit)

        // 400–499 API & Edge
        Api             = 410, // controllers/handlers
        Gateway         = 420, // reverse proxy, edge auth, rate limits
        Webhook         = 430, // inbound/outbound webhooks

        // 500–599 Data
        Data            = 510, // domain CRUD, repositories
        Storage         = 520, // blob/file stores
        Cache           = 530, // hits/misses/evictions
        Search          = 540, // indexes/queries
        Migration       = 550, // schema/data migrations, ETL
        Analytics       = 560, // reporting, aggregates

        // 600–699 Integrations
        Integration     = 610, // generic third-party calls
        PaymentsApi     = 620, // payment provider SDK/API calls
        Messaging       = 630, // SMS/push, brokers
        Email           = 640, // SMTP/providers

        // 700–799 Automation & Workflow
        Automation      = 710, // background jobs
        Scheduler       = 720, // cron/timers
        Workflow        = 730, // business workflows, steps
        Job             = 740, // discrete job units
        Queue           = 750, // enqueue/dequeue, retries

        // 800–879 System & Platform
        Sys             = 810, // service lifecycle, OS/container
        Config          = 820, // config changes, secrets wiring
        Deployment      = 830, // releases, rollbacks
        FeatureFlag     = 840, // platform feature toggles
        Scaling         = 850, // autoscale, capacity
        Health          = 860, // health/liveness/readiness

        // 880–899 Observability & Performance
        Perf            = 885, // timings, budgets, p50/p95
        Telemetry       = 890, // traces/metrics/log plumbing
        Alerting        = 895, // alerts, paging, runbooks

        // 900–949 Commercial & Finance (expanded)
        Commerce        = 900, // umbrella for commercial flows
        Pricing         = 902, // price books, tiering, CPI/GST adjustments
        Quoting         = 904, // quotes/estimates, validity windows
        Discounts       = 906, // discount rules, application, revocation
        Promotions      = 908, // promo codes, campaigns, eligibility
        Tax             = 910, // GST/VAT/sales tax calc, nexus, exemptions
        Invoicing       = 912, // invoice generation, numbering, PDFs
        BillingCycle    = 914, // cycles, proration, cutoff, true-up
        Payments        = 916, // captures, confirms, auths
        Refunds         = 918, // full/partial refunds
        Credits         = 920, // credit notes, stored credits
        Debits          = 922, // debit memos, manual debits
        Adjustments     = 924, // one-off adjustments, write-offs
        Fees            = 926, // late fees, service fees, surcharges
        Disputes        = 928, // chargebacks, evidence, outcomes
        Settlement      = 930, // payouts/transfers, partner splits
        Reconciliation  = 932, // bank vs ledger, unmatched items
        RevenueShare    = 934, // partner/vendor/TMC splits, commissions
        Subscriptions   = 936, // recurring plans, seats, renewals, cancels
        AccountingExport= 938, // GL export, journal posting, Xero/NetSuite
        CurrencyFx      = 940, // FX rates, conversions, rounding
        Collections     = 942, // dunning, reminders, recovery workflows
        // 944–949 reserved for future commercial categories

        // 990–999 Misc
        Other           = 990  // uncategorised; minimise usage
    }
}
