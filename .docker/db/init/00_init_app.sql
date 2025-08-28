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
--     Customize these three variables to your desired creds/names.
-- ─────────────────────────────────────────────────────────────────────────────
\set loguser   'serilog'                 -- role used by your apps for logging
\set logpass   'YourSerilogPassword'     -- strong password for the logging role
\set logschema 'serilog'                 -- schema to hold the logs table

\echo 🧑‍💻 [serilog] ensure role :loguser exists (create if missing)
SELECT format(
  'CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT',
  :'loguser', :'logpass'
)
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'loguser')
\gexec

\echo 🔐  [serilog] enforce SCRAM, LOGIN, password, no expiry
SET password_encryption = 'scram-sha-256';
ALTER ROLE :"loguser" LOGIN;
ALTER ROLE :"loguser" WITH PASSWORD :'logpass';
ALTER ROLE :"loguser" VALID UNTIL 'infinity';

\echo 🔌  [serilog] allow :loguser to connect to :appdb
GRANT CONNECT ON DATABASE :"appdb" TO :"loguser";

\echo 🏗️  [serilog] create schema :logschema owned by :loguser (if missing)
SELECT format('CREATE SCHEMA IF NOT EXISTS %I AUTHORIZATION %I', :'logschema', :'loguser')
\gexec

\echo 🧱  [serilog] create table :logschema.logs (if missing)
SELECT format($$CREATE TABLE IF NOT EXISTS %I.logs (
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
)$$, :'logschema')
\gexec

\echo 🧭  [serilog] indexes for common filters
SELECT format('CREATE INDEX IF NOT EXISTS %I ON %I.logs ("timestamp" DESC)', 'ix_logs_timestamp', :'logschema') \gexec
SELECT format('CREATE INDEX IF NOT EXISTS %I ON %I.logs (level)',          'ix_logs_level',     :'logschema') \gexec

\echo 🎟️  [serilog] grants for logging role
SELECT format('GRANT USAGE ON SCHEMA %I TO %I', :'logschema', :'loguser') \gexec
SELECT format('GRANT INSERT, SELECT ON ALL TABLES IN SCHEMA %I TO %I', :'logschema', :'loguser') \gexec
SELECT format('GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA %I TO %I', :'logschema', :'loguser') \gexec

-- (Optional) ensure future objects in the serilog schema grant to :loguser
ALTER DEFAULT PRIVILEGES IN SCHEMA :logschema GRANT INSERT, SELECT ON TABLES TO :"loguser";
ALTER DEFAULT PRIVILEGES IN SCHEMA :logschema GRANT USAGE, SELECT ON SEQUENCES TO :"loguser";


\echo ✅  [init] done
