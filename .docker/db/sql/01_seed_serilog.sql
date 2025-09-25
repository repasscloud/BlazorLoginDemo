-- 01_seed_serilog.sql
-- Dedicated role/schema/table for Serilog logging

DO $$
DECLARE
  v_loguser   text := 'serilog';
  v_logpass   text := 'YourSerilogPassword'; -- CHANGE THIS
  v_logschema text := 'serilog';
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

  -- Allow connect to the current database
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
