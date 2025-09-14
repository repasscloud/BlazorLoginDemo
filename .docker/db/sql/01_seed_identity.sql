-- Enable uuid generator
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 1) Roles (upsert on NormalizedName)
INSERT INTO "AspNetRoles" ("Id","Name","NormalizedName","ConcurrencyStamp")
SELECT gen_random_uuid()::text, r.name, upper(r.name), gen_random_uuid()::text
FROM (VALUES
  ('SuperAdmin'),
  ('Auditor'),
  ('UserAdmin'),
  ('PolicyAdmin'),
  ('FinanceAdmin'),
  ('FinanceEditor'),
  ('FinanceViewer'),
  ('SecurityAdmin'),
  ('IntegrationAdmin'),
  ('SalesRep'),
  ('SalesManager'),
  ('SalesAdmin'),
  ('SupportViewer'),
  ('SupportAgent'),
  ('SupportFinance'),
  ('SupportAdmin'),
  ('ReportsViewer'),
  ('DataExporter'),
  ('Requestor'),
  ('ReadOnly'),
  ('OrgAdmin'),
  ('OrgUserAdmin'),
  ('OrgPolicyAdmin'),
  ('OrgFinanceAdmin'),
  ('OrgBookingsManager'),
  ('OrgApproverL1'),
  ('OrgApproverL2'),
  ('OrgApproverL3'),
  ('OrgReportsViewer'),
  ('OrgDataExporter')
) AS r(name)
ON CONFLICT ("NormalizedName") DO NOTHING;

-- 2) Admin user (admin@example.com) and assign SuperAdmin role
DO $$
DECLARE
  v_user_id  text;
  v_role_id  text;
BEGIN
  -- make sure pgcrypto is available for gen_random_uuid()
  PERFORM 1 FROM pg_extension WHERE extname = 'pgcrypto';
  IF NOT FOUND THEN
    CREATE EXTENSION IF NOT EXISTS pgcrypto;
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM "AspNetUsers"
    WHERE "NormalizedEmail" = 'ADMIN@EXAMPLE.COM'
  ) THEN
    v_user_id := gen_random_uuid()::text;

    INSERT INTO "AspNetUsers" (
      "Id",
      "UserName","NormalizedUserName",
      "Email","NormalizedEmail","EmailConfirmed",
      "PasswordHash","SecurityStamp","ConcurrencyStamp",
      "PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnabled","AccessFailedCount",
      "DisplayName","FirstName","LastName","Department","IsActive","LastSeenUtc",
      "UserCategory"
    )
    VALUES (
      v_user_id,
      'admin@example.com','ADMIN@EXAMPLE.COM',
      'admin@example.com','ADMIN@EXAMPLE.COM', TRUE,
      'AQAAAAIAAYagAAAAEBE3ewtQgCa2KtXOdZWOXqPTJJ20RKzi0d5luHfed9lucXiJ4aJ6XO8tSvb4FROWYg==',
      gen_random_uuid()::text,
      gen_random_uuid()::text,
      FALSE, FALSE, FALSE, 0,
      'Administrator','BuiltIn','Administrator', NULL, TRUE, NULL,
      0   -- 👈 hard-coded UserCategory = 0
    );

    SELECT "Id" INTO v_role_id
    FROM "AspNetRoles"
    WHERE "NormalizedName" = 'SUPERADMIN'
    LIMIT 1;

    IF v_role_id IS NOT NULL THEN
      INSERT INTO "AspNetUserRoles" ("UserId","RoleId")
      VALUES (v_user_id, v_role_id)
      ON CONFLICT DO NOTHING;
    END IF;
  END IF;
END$$;
