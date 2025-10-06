-- =========================================================
-- Script: link_organizations_hierarchy.sql
-- Purpose:
--   Establish parent/child hierarchy between organizations.
--   RePass Cloud Pty Ltd → New World Travel Management → McDonald's Australia Limited
--
-- Notes:
--   - Assumes all organization names exist in ava.organizations.
--   - Uses dynamic lookup of IDs, no hardcoded values.
--   - Automatically runs in a single transaction scope.
--   - No explicit COMMIT required unless run inside a larger transaction.
-- =========================================================

DO $$
DECLARE
    vendor_org_id text;  -- RePass Cloud Pty Ltd
    tmc_org_id text;     -- New World Travel Management
BEGIN
    -- Get parent (vendor) org ID
    SELECT "Id" INTO vendor_org_id
    FROM ava.organizations
    WHERE "Name" = 'RePass Cloud Pty Ltd';

	-- Link SUDO to RePass Cloud
    UPDATE public."AspNetUsers"
    SET "OrganizationId" = vendor_org_id
    WHERE "UserName" = 'sudo@localhost.com';
	
    -- Link New World Travel Management to RePass Cloud
    UPDATE ava.organizations
    SET "ParentOrganizationId" = vendor_org_id
    WHERE "Name" = 'New World Travel Management';

    -- Get TMC org ID
    SELECT "Id" INTO tmc_org_id
    FROM ava.organizations
    WHERE "Name" = 'New World Travel Management';

    -- Link McDonald's Australia to New World Travel Management
    UPDATE ava.organizations
    SET "ParentOrganizationId" = tmc_org_id
    WHERE "Name" = 'MCDONALD''S AUSTRALIA LIMITED';
END $$;
