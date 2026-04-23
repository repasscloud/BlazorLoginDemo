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
-- Create SUDO user for new AspNetUsers schema and assign SUDO role
DO $$
DECLARE
  v_user_id  text;
  v_role_id  text;
BEGIN
  -- Ensure pgcrypto for gen_random_uuid()
  PERFORM 1 FROM pg_extension WHERE extname = 'pgcrypto';
  IF NOT FOUND THEN
    CREATE EXTENSION IF NOT EXISTS pgcrypto;
  END IF;

  -- Only create if it doesn't already exist (by normalized email)
  IF NOT EXISTS (
    SELECT 1 FROM public."AspNetUsers"
    WHERE "NormalizedEmail" = 'SUDO@LOCALHOST.COM'
  ) THEN
    v_user_id := gen_random_uuid()::text;

    INSERT INTO public."AspNetUsers" (
      -- Identity/Account fields (standard)
      "Id",
      "UserName","NormalizedUserName",
      "Email","NormalizedEmail","EmailConfirmed",
      "PasswordHash","SecurityStamp","ConcurrencyStamp",
      "PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnabled","AccessFailedCount",

      -- Your extended profile fields
      "IsActive","LastSeenUtc","DisplayName","FirstName","LastName","Department",
      "UserCategory","PreferredCulture","CostCentre","Gender","CountryOfIssue",

      -- Travel / policy defaults (satisfy NOT NULLs)
      "DefaultFlightSeating","DefaultFlightSeatingVisible",
      "MaxFlightSeating","MaxFlightSeatingVisible",
      -- Included/ExcludedAirlineCodes are NOT NULL but have defaults -> omit to use '{}'::text[]
      "AirlineCodesVisible",
      "CabinClassCoverage","CabinClassCoverageVisible",
      -- "NonStopFlight" is nullable; but its *Visible is NOT NULL
      "NonStopFlightVisible",
      "DefaultCurrencyCode","DefaultCurrencyCodeVisible",
      "MaxFlightPrice","MaxFlightPriceVisible",
      "MaxResults","MaxResultsVisible",
      -- time windows are nullable; but its *Visible is NOT NULL
      "FlightBookingTimeAvailableVisible",
      -- weekend toggles are nullable; but its *Visible is NOT NULL
      "EnableWeekendFlightBookingsVisible",
      -- calendar days nullable; but its *Visible is NOT NULL
      "CalendarDaysInAdvanceForFlightBookingVisible"
    )
    VALUES (
      -- Identity/Account fields
      v_user_id,
      'sudo@localhost.com','SUDO@LOCALHOST.COM',
      'sudo@localhost.com','SUDO@LOCALHOST.COM', TRUE,
      'AQAAAAIAAYagAAAAEBE3ewtQgCa2KtXOdZWOXqPTJJ20RKzi0d5luHfed9lucXiJ4aJ6XO8tSvb4FROWYg==',
      gen_random_uuid()::text,
      gen_random_uuid()::text,
      FALSE, FALSE, FALSE, 0,

      -- Extended profile
      TRUE,            -- IsActive (NOT NULL)
      NULL,            -- LastSeenUtc (nullable)
      'sudo',          -- DisplayName
      'Sudo',          -- FirstName
      'Administrator', -- LastName
      NULL,            -- Department (nullable)
      0,               -- UserCategory (NOT NULL) -> 0 = default category
      'en-AU',         -- PreferredCulture (NOT NULL)
      NULL,            -- CostCentre (nullable)
      0,               -- Gender (NOT NULL) -> align with your enum's "Unknown"
      0,               -- CountryOfIssue (NOT NULL) -> align with your enum/country table (e.g., 0=Unknown)

      -- Travel / policy defaults (pick safe system defaults)
      'FIRST',         -- DefaultFlightSeating (NOT NULL): 'Any'/'Aisle'/'Window' as your app expects
      FALSE,           -- DefaultFlightSeatingVisible (NOT NULL)
      'FIRST',         -- MaxFlightSeating (NOT NULL)
      FALSE,           -- MaxFlightSeatingVisible (NOT NULL)
      FALSE,           -- AirlineCodesVisible (NOT NULL)
      'ALL_SEGMENTS',  -- CabinClassCoverage (NOT NULL)
      FALSE,           -- CabinClassCoverageVisible (NOT NULL)
      FALSE,           -- NonStopFlightVisible (NOT NULL)
      'AUD',           -- DefaultCurrencyCode (NOT NULL)
      FALSE,           -- DefaultCurrencyCodeVisible (NOT NULL)
      0.0,             -- MaxFlightPrice (NOT NULL)
      FALSE,           -- MaxFlightPriceVisible (NOT NULL)
      50,              -- MaxResults (NOT NULL)
      FALSE,           -- MaxResultsVisible (NOT NULL)
      FALSE,           -- FlightBookingTimeAvailableVisible (NOT NULL)
      FALSE,           -- EnableWeekendFlightBookingsVisible (NOT NULL)
      FALSE            -- CalendarDaysInAdvanceForFlightBookingVisible (NOT NULL)
    );

    -- Link SUDO role if it exists
    SELECT "Id" INTO v_role_id
    FROM public."AspNetRoles"
    WHERE "NormalizedName" = 'SUDO'
    LIMIT 1;

    IF v_role_id IS NOT NULL THEN
      INSERT INTO public."AspNetUserRoles" ("UserId","RoleId")
      VALUES (v_user_id, v_role_id)
      ON CONFLICT DO NOTHING;
    END IF;
  END IF;
END$$;
