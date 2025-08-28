-- .docker/db/init/00_init_app.sql
\set ON_ERROR_STOP on
\set appdb   'demodb'
\set appuser 'demodb'
\set apppass 'YourAppPassword'

\echo 🌱  [init] connect as postgres
\connect postgres

\echo 👤  [init] ensure role :appuser exists (create if missing)
SELECT format(
  'CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT',
  :'appuser', :'apppass'
)
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'appuser')
\gexec

\echo 🔐  [init] enforce SCRAM, LOGIN, password, no expiry
SET password_encryption = 'scram-sha-256';
ALTER ROLE :"appuser" LOGIN;
ALTER ROLE :"appuser" WITH PASSWORD :'apppass';
ALTER ROLE :"appuser" VALID UNTIL 'infinity';

\echo 🗄️  [init] ensure database :appdb exists (create if missing)
SELECT format(
  'CREATE DATABASE %I WITH OWNER = %I TEMPLATE = template0 ENCODING = %L LC_COLLATE = %L LC_CTYPE = %L',
  :'appdb', :'appuser', 'UTF8', 'en_US.UTF-8', 'en_US.UTF-8'
)
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = :'appdb')
\gexec

\echo 🏷️  [init] ensure DB owner/privs
ALTER DATABASE :"appdb" OWNER TO :"appuser";
GRANT CONNECT ON DATABASE :"appdb" TO :"appuser";

\echo 🔄  [init] switch to app DB
\connect :appdb

\echo 🧰  [init] schema ownership + default privileges
ALTER SCHEMA public OWNER TO :"appuser";
GRANT USAGE, CREATE ON SCHEMA public TO :"appuser";
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO :"appuser";
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO :"appuser";
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO :"appuser";

\echo 📜  [init] EF migrations history table (owned by app user)
CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory"(
  "MigrationId" varchar(150) NOT NULL,
  "ProductVersion" varchar(32) NOT NULL,
  CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
ALTER TABLE public."__EFMigrationsHistory" OWNER TO :"appuser";


-- ─────────────────────────────────────────────────────────────────────────────
-- 🧾  SERILOG dedicated role + schema + table (within the SAME :appdb)
--     Rewritten using a single plpgsql DO block (no \gexec needed here).
--     Customize these three variables as you like.
-- ─────────────────────────────────────────────────────────────────────────────
\set loguser   'serilog'                 -- role used by your apps for logging
\set logpass   'YourSerilogPassword'     -- strong password for the logging role
\set logschema 'serilog'                 -- schema to hold the logs table

\echo 🧑‍💻 [serilog] ensure role :loguser exists (create if missing)

DO $$
DECLARE
  v_loguser   text := :'loguser';
  v_logpass   text := :'logpass';
  v_logschema text := :'logschema';
BEGIN
  -- Create logging role if missing
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = v_loguser) THEN
    EXECUTE format(
      'CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT',
      v_loguser, v_logpass
    );
  END IF;

  -- Enforce login/password/expiry
  EXECUTE format('ALTER ROLE %I LOGIN', v_loguser);
  EXECUTE format('ALTER ROLE %I PASSWORD %L', v_loguser, v_logpass);
  EXECUTE format('ALTER ROLE %I VALID UNTIL %L', v_loguser, 'infinity');

  -- Allow connect to *this* database
  EXECUTE format('GRANT CONNECT ON DATABASE %I TO %I', current_database(), v_loguser);

  -- Create schema owned by the logging role
  EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I AUTHORIZATION %I', v_logschema, v_loguser);

  -- Create logs table
  EXECUTE format($fmt$
    CREATE TABLE IF NOT EXISTS %I.logs (
      id                BIGSERIAL PRIMARY KEY,
      "timestamp"       TIMESTAMPTZ NOT NULL DEFAULT now(),
      level             VARCHAR(128) NOT NULL,
      message           TEXT NULL,
      message_template  TEXT NULL,
      exception         TEXT NULL,
      properties        JSONB NULL,
      request_path      VARCHAR(512) NULL,
      request_id        VARCHAR(128) NULL,
      user_id           VARCHAR(128) NULL,
      source_context    VARCHAR(256) NULL,
      environment       VARCHAR(64) NULL,
      application       VARCHAR(64) NULL
    )
  $fmt$, v_logschema);

  -- Indexes
  EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON %I.logs ("timestamp" DESC)', 'ix_logs_timestamp', v_logschema);
  EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON %I.logs (level)',            'ix_logs_level',     v_logschema);

  -- Grants
  EXECUTE format('GRANT USAGE ON SCHEMA %I TO %I', v_logschema, v_loguser);
  EXECUTE format('GRANT INSERT, SELECT ON ALL TABLES IN SCHEMA %I TO %I', v_logschema, v_loguser);
  EXECUTE format('GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA %I TO %I', v_logschema, v_loguser);

  -- Default privileges for future objects in the serilog schema
  EXECUTE format('ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT INSERT, SELECT ON TABLES TO %I', v_logschema, v_loguser);
  EXECUTE format('ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT USAGE, SELECT ON SEQUENCES TO %I', v_logschema, v_loguser);
END
$$ LANGUAGE plpgsql;

\echo ✅  [init] done
