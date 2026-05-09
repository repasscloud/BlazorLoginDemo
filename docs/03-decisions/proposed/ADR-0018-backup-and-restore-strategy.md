# ADR-0018: Backup and Restore Strategy

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

There is currently no documented backup or restore strategy for Cinturon360. The `deploy/` tree contains only a dev compose file. No production infrastructure exists yet. However, defining the strategy now is important because:
- Schema migrations are applied with `psql` from idempotent SQL scripts; there is no automatic rollback if a migration goes wrong.
- The billing domain holds financial records (`BillingInvoice`, `JournalEntry`, `PaymentAttempt`) that have regulatory retention requirements.
- The secrets managed by Azure Key Vault (JWT signing key, Stripe webhook secrets, API keys) must also be backed up.
- A developer accidentally applied an empty migration (`YourMigrationName`) to a shared database, demonstrating that operational mistakes can reach the database.

The database will run on Azure Database for PostgreSQL Flexible Server (per ADR-0017). This service provides:
- Automated backups with point-in-time restore (PITR) — default 7 days, configurable to 35 days.
- Geo-redundant backup (optional; higher cost).
- Manual on-demand backups via the Azure portal or CLI.

## Decision

*Not yet decided.* The decision must specify:

1. **Recovery Point Objective (RPO):** Maximum acceptable data loss in the event of a failure.
   - Recommended: 1 hour for production (achievable with Azure Flexible Server PITR).

2. **Recovery Time Objective (RTO):** Maximum acceptable time to restore service.
   - Recommended: 4 hours for production.

3. **Backup retention:**
   - Production: 35 days PITR (maximum for Azure Flexible Server).
   - Staging: 7 days PITR (default).
   - Development: no automated backup (local volume, `dev-start.sh` is the recovery).

4. **Geo-redundancy:** Whether production backups are stored in a paired Azure region.
   - Recommended: Yes, for financial data compliance.

5. **Secret backup:** Azure Key Vault soft-delete and purge protection must be enabled. Key Vault contents must be backed up to a secondary Key Vault in a paired region.

6. **Application-level export:** In addition to PITR, a scheduled `pg_dump` export to Azure Storage provides a portable, application-consistent backup. Define the frequency (daily recommended) and retention (90 days recommended).

7. **Restore runbook:** A documented, tested procedure for PITR restore and `pg_dump` restore.

8. **Dev migrate-script recovery:** The `tools/db/migrations/` SQL scripts must be kept consistent with the actual migration history. The duplicate `003_*.sql` files must be resolved (see migration debt in the technical debt section).

## Consequences

**If these decisions are accepted:**
- Enable 35-day PITR on the production PostgreSQL Flexible Server in Bicep.
- Enable geo-redundant backup in Bicep (`backupRetentionDays: 35`, `geoRedundantBackup: "Enabled"`).
- Add a scheduled job (`PgDumpExportJob`) to `Cinturon360.Jobs` that runs `pg_dump` and uploads the result to Azure Blob Storage on a daily schedule.
- Enable Key Vault soft-delete (`softDeleteRetentionInDays: 90`) and purge protection in Bicep.
- Write `docs/10-runbooks/database/restore-pitr.md` and `docs/10-runbooks/database/restore-pgdump.md`.
- Resolve the duplicate `003_*.sql` files in `tools/db/migrations/`.
- Define alerting: if the daily `pg_dump` job fails, a Slack/email alert must fire within 1 hour.

**Required follow-up:**
- Confirm RPO/RTO targets with stakeholders.
- Confirm Azure region pair (e.g., Australia East + Australia Southeast).
- Test a PITR restore in staging before first production deployment.
- Author restore runbooks.
