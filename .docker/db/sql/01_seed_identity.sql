-- Enable uuid generator
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 1) Roles (upsert on NormalizedName)
INSERT INTO "AspNetRoles" ("Id","Name","NormalizedName","ConcurrencyStamp")
SELECT gen_random_uuid()::text, r.name, upper(r.name), gen_random_uuid()::text
FROM (VALUES
  ('Sudo'),
  ('Platform.SuperAdmin'),
  ('Platform.SuperUser'),
  ('Platform.Admin'),
  ('Platform.Support.Admin'),
  ('Platform.Support.Agent'),
  ('Platform.Support.Viewer'),
  ('Platform.Support.Finance'),
  ('Platform.UserAdmin'),
  ('Platform.OrgAdmin'),
  ('Platform.PolicyAdmin'),
  ('Platform.SecurityAdmin'),
  ('Platform.IntegrationAdmin'),
  ('Platform.Finance.Admin'),
  ('Platform.Finance.Editor'),
  ('Platform.Finance.Viewer'),
  ('Platform.Sales.Rep'),
  ('Platform.Sales.Manager'),
  ('Platform.Sales.Admin'),
  ('Platform.ReportsViewer'),
  ('Platform.DataExporter'),
  ('Platform.Auditor'),
  ('Platform.ReadOnly'),
  ('Tmc.Admin'),
  ('Tmc.UserAdmin'),
  ('Tmc.PolicyAdmin'),
  ('Tmc.SecurityAdmin'),
  ('Tmc.IntegrationAdmin'),
  ('Tmc.Finance.Admin'),
  ('Tmc.Finance.Editor'),
  ('Tmc.Finance.Viewer'),
  ('Tmc.BookingsManager'),
  ('Tmc.TravelAgent'),
  ('Tmc.ReportsViewer'),
  ('Tmc.DataExporter'),
  ('Tmc.Auditor'),
  ('Tmc.ReadOnly'),
  ('Client.Admin'),
  ('Client.UserAdmin'),
  ('Client.PolicyAdmin'),
  ('Client.SecurityAdmin'),
  ('Client.IntegrationAdmin'),
  ('Client.Finance.Admin'),
  ('Client.Finance.Editor'),
  ('Client.Finance.Viewer'),
  ('Client.Approver.L1'),
  ('Client.Approver.L2'),
  ('Client.Approver.L3'),
  ('Client.ReportsViewer'),
  ('Client.DataExporter'),
  ('Client.Auditor'),
  ('Client.ReadOnly'),
  ('Client.Requestor')
) AS r(name)
ON CONFLICT ("NormalizedName") DO NOTHING;

-- 2) Admin user (sudo@localhost.com) and assign SuperAdmin role
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
    WHERE "NormalizedEmail" = 'SUDO@LOCALHOST.COM'
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
      'sudo@localhost.com','SUDO@LOCALHOST.COM',
      'sudo@localhost.com','SUDO@LOCALHOST.COM', TRUE,
      'AQAAAAIAAYagAAAAEBE3ewtQgCa2KtXOdZWOXqPTJJ20RKzi0d5luHfed9lucXiJ4aJ6XO8tSvb4FROWYg==',
      gen_random_uuid()::text,
      gen_random_uuid()::text,
      FALSE, FALSE, FALSE, 0,
      'sudo','Sudo','Administrator', NULL, TRUE, NULL,
      0   -- 👈 hard-coded UserCategory = 0
    );

    SELECT "Id" INTO v_role_id
    FROM "AspNetRoles"
    WHERE "NormalizedName" = 'SUDO'
    LIMIT 1;

    IF v_role_id IS NOT NULL THEN
      INSERT INTO "AspNetUserRoles" ("UserId","RoleId")
      VALUES (v_user_id, v_role_id)
      ON CONFLICT DO NOTHING;
    END IF;
  END IF;
END$$;
