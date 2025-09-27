/* ============================================================================
   Purpose: Backfill org → license 1:1 foreign key
   Where:   Postgres (schema: ava)
   What:    Populate ava.organizations."LicenseAgreementId" from
            ava.license_agreements."OrganizationUnifiedId" for any orgs
            that currently have a NULL license link.

   Why:     Earlier versions created LicenseAgreement rows without updating
            the organization’s FK column. This script restores the back-link.

   Safety:  Idempotent — only fills NULLs; safe to re-run.
   Impact:  Writes to ava.organizations only (no data loss).
   When:    After deploying the service changes that keep the link in sync.

   Verification (optional):
     -- How many will be updated?
     -- SELECT COUNT(*) 
     -- FROM ava.organizations o
     -- JOIN ava.license_agreements la ON la."OrganizationUnifiedId" = o."Id"
     -- WHERE o."LicenseAgreementId" IS NULL;

   Rollback (manual, if needed):
     -- To undo only the rows touched in this run, capture IDs first:
     -- BEGIN;
     -- CREATE TEMP TABLE _touched AS
     --   SELECT o."Id"
     --   FROM ava.organizations o
     --   JOIN ava.license_agreements la ON la."OrganizationUnifiedId" = o."Id"
     --   WHERE o."LicenseAgreementId" IS NULL;
     -- UPDATE ava.organizations o
     -- SET "LicenseAgreementId" = NULL
     -- FROM _touched t
     -- WHERE o."Id" = t."Id";
     -- COMMIT;

   Post-check (optional):
     -- Any orgs still missing a link despite having a license?
     -- SELECT o."Id"
     -- FROM ava.organizations o
     -- JOIN ava.license_agreements la ON la."OrganizationUnifiedId" = o."Id"
     -- WHERE o."LicenseAgreementId" IS NULL;

   ========================================================================== */

BEGIN;

UPDATE ava.organizations o
SET "LicenseAgreementId" = la."Id"
FROM ava.license_agreements la
WHERE la."OrganizationUnifiedId" = o."Id"
  AND o."LicenseAgreementId" IS NULL;

COMMIT;
