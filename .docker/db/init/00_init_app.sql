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

\echo ✅  [init] done
