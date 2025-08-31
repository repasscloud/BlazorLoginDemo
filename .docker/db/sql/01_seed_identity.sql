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

-- 2) Catch-all GroupX (upsert on Name)
INSERT INTO "Groups" ("Id","Name","IsCatchAll","IsActive","CreatedUtc")
VALUES (gen_random_uuid(), 'GroupX', TRUE, TRUE, now())
ON CONFLICT ("Name") DO NOTHING;


-- 3) Admin user (admin@example.com) and assign SuperAdmin role
DO $$
DECLARE
  v_user_id  text;
  v_role_id  text;
  v_group_id uuid;
BEGIN
  SELECT "Id" INTO v_group_id FROM "Groups" WHERE "Name"='GroupX' LIMIT 1;

  IF NOT EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "NormalizedEmail"='ADMIN@EXAMPLE.COM') THEN
    v_user_id := gen_random_uuid()::text;

    INSERT INTO "AspNetUsers" (
      "Id","UserName","NormalizedUserName","Email","NormalizedEmail","EmailConfirmed",
      "PasswordHash","SecurityStamp","ConcurrencyStamp",
      "PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnabled","AccessFailedCount",
      "DisplayName","Department","IsActive","LastSeenUtc","GroupId"
    ) VALUES (
      v_user_id,
      'admin@example.com','ADMIN@EXAMPLE.COM',
      'admin@example.com','ADMIN@EXAMPLE.COM',
      TRUE,
      'AQAAAAIAAYagAAAAEBE3ewtQgCa2KtXOdZWOXqPTJJ20RKzi0d5luHfed9lucXiJ4aJ6XO8tSvb4FROWYg==',
      gen_random_uuid()::text,
      gen_random_uuid()::text,
      FALSE, FALSE, FALSE, 0,
      'Administrator', NULL, TRUE, NULL, v_group_id
    );

    SELECT "Id" INTO v_role_id FROM "AspNetRoles" WHERE "NormalizedName"='SUPERADMIN' LIMIT 1;
    IF v_role_id IS NOT NULL THEN
      INSERT INTO "AspNetUserRoles" ("UserId","RoleId")
      VALUES (v_user_id, v_role_id)
      ON CONFLICT DO NOTHING;
    END IF;
  END IF;
END$$;
