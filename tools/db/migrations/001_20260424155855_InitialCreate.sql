CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE "OpenIddictApplications" (
        id text NOT NULL,
        application_type character varying(50),
        client_id character varying(100),
        client_secret text,
        client_type character varying(50),
        concurrency_token character varying(50),
        consent_type character varying(50),
        display_name text,
        display_names text,
        json_web_key_set text,
        permissions text,
        post_logout_redirect_uris text,
        properties text,
        redirect_uris text,
        requirements text,
        settings text,
        CONSTRAINT pk_open_iddict_applications PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE "OpenIddictScopes" (
        id text NOT NULL,
        concurrency_token character varying(50),
        description text,
        descriptions text,
        display_name text,
        display_names text,
        name character varying(200),
        properties text,
        resources text,
        CONSTRAINT pk_open_iddict_scopes PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE organisations (
        id character varying(50) NOT NULL,
        name character varying(200) NOT NULL,
        slug character varying(100) NOT NULL,
        org_type integer NOT NULL,
        parent_org_id character varying(50),
        is_active boolean NOT NULL,
        logo_storage_key character varying(500),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_organisations PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE role_permissions (
        id character varying(50) NOT NULL,
        role_id character varying(50) NOT NULL,
        permission_code character varying(100) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_role_permissions PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE roles (
        id character varying(50) NOT NULL,
        org_type integer NOT NULL,
        name character varying(100) NOT NULL,
        description character varying(500),
        is_system_role boolean NOT NULL,
        org_id character varying(50),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_roles PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_access_overrides (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        policy_key character varying(100) NOT NULL,
        policy_value character varying(500) NOT NULL,
        granted_by_user_id character varying(50),
        expires_at timestamp with time zone,
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_access_overrides PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_api_tokens (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        token_class integer NOT NULL,
        name character varying(200) NOT NULL,
        token_hash character varying(500) NOT NULL,
        token_prefix character varying(30) NOT NULL,
        expires_at timestamp with time zone,
        is_revoked boolean NOT NULL,
        revoked_at timestamp with time zone,
        last_used_at timestamp with time zone,
        scopes character varying(2000),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_api_tokens PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_audit_events (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        event_type integer NOT NULL,
        actor_user_id character varying(50),
        org_id character varying(50),
        ip_address character varying(45),
        user_agent character varying(500),
        correlation_id character varying(100),
        details jsonb,
        success boolean NOT NULL,
        failure_reason character varying(1000),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_audit_events PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_auth_methods (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        login_method integer NOT NULL,
        status integer NOT NULL,
        external_subject character varying(500),
        provider_config_id character varying(50),
        last_used_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_auth_methods PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_mfa_methods (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        mfa_method integer NOT NULL,
        is_enabled boolean NOT NULL,
        secret_or_target character varying(1000),
        last_used_at timestamp with time zone,
        verified_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_mfa_methods PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_provisioning_sources (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        source integer NOT NULL,
        source_identifier character varying(500),
        batch_id character varying(100),
        provisioned_by_user_id character varying(50),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_provisioning_sources PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_recovery_codes (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        code_hash character varying(500) NOT NULL,
        is_used boolean NOT NULL,
        used_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_recovery_codes PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_role_assignments (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        role_id character varying(50) NOT NULL,
        scope_mode integer NOT NULL,
        is_active boolean NOT NULL,
        granted_by_user_id character varying(50),
        expires_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_role_assignments PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_security (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        password_hash character varying(1000),
        password_changed_at timestamp with time zone,
        password_expires_at timestamp with time zone,
        is_mfa_enabled boolean NOT NULL,
        is_mfa_required boolean NOT NULL,
        allow_direct_login boolean NOT NULL,
        failed_login_attempts integer NOT NULL,
        lockout_until timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_security PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE user_sessions (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        token_class integer NOT NULL,
        jti character varying(200) NOT NULL,
        device_id character varying(200),
        user_agent character varying(500),
        ip_address character varying(45),
        is_revoked boolean NOT NULL,
        revoked_at timestamp with time zone,
        revoked_reason character varying(500),
        expires_at timestamp with time zone NOT NULL,
        last_active_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_sessions PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE users (
        id character varying(50) NOT NULL,
        user_category integer NOT NULL,
        platform_role integer,
        home_org_id character varying(50),
        email character varying(256) NOT NULL,
        first_name character varying(100) NOT NULL,
        last_name character varying(100) NOT NULL,
        is_email_verified boolean NOT NULL,
        is_active boolean NOT NULL,
        is_locked boolean NOT NULL,
        is_suspended boolean NOT NULL,
        language_code character varying(10) NOT NULL DEFAULT 'en',
        time_zone character varying(64) NOT NULL DEFAULT 'UTC',
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        avatar_storage_key character varying(500),
        last_login_at timestamp with time zone,
        concurrency_stamp character varying(50) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_users PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE "OpenIddictAuthorizations" (
        id text NOT NULL,
        application_id text,
        concurrency_token character varying(50),
        creation_date timestamp with time zone,
        properties text,
        scopes text,
        status character varying(50),
        subject character varying(400),
        type character varying(50),
        CONSTRAINT pk_open_iddict_authorizations PRIMARY KEY (id),
        CONSTRAINT fk_open_iddict_authorizations_open_iddict_applications_application FOREIGN KEY (application_id) REFERENCES "OpenIddictApplications" (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE TABLE "OpenIddictTokens" (
        id text NOT NULL,
        application_id text,
        authorization_id text,
        concurrency_token character varying(50),
        creation_date timestamp with time zone,
        expiration_date timestamp with time zone,
        payload text,
        properties text,
        redemption_date timestamp with time zone,
        reference_id character varying(100),
        status character varying(50),
        subject character varying(400),
        type character varying(50),
        CONSTRAINT pk_open_iddict_tokens PRIMARY KEY (id),
        CONSTRAINT fk_open_iddict_tokens_open_iddict_applications_application_id FOREIGN KEY (application_id) REFERENCES "OpenIddictApplications" (id),
        CONSTRAINT fk_open_iddict_tokens_open_iddict_authorizations_authorization_id FOREIGN KEY (authorization_id) REFERENCES "OpenIddictAuthorizations" (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_open_iddict_applications_client_id ON "OpenIddictApplications" (client_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_open_iddict_authorizations_application_id_status_subject_type ON "OpenIddictAuthorizations" (application_id, status, subject, type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_open_iddict_scopes_name ON "OpenIddictScopes" (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_open_iddict_tokens_application_id_status_subject_type ON "OpenIddictTokens" (application_id, status, subject, type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_open_iddict_tokens_authorization_id ON "OpenIddictTokens" (authorization_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_open_iddict_tokens_reference_id ON "OpenIddictTokens" (reference_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_organisations_is_active ON organisations (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_organisations_org_type ON organisations (org_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_organisations_parent_org_id ON organisations (parent_org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_organisations_slug ON organisations (slug);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_role_permissions_role_id ON role_permissions (role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_role_permissions_role_id_permission_code ON role_permissions (role_id, permission_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_roles_org_id ON roles (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_roles_org_type_is_system_role ON roles (org_type, is_system_role);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_access_overrides_user_id ON user_access_overrides (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_access_overrides_user_id_policy_key_is_active ON user_access_overrides (user_id, policy_key, is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_api_tokens_token_hash ON user_api_tokens (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_api_tokens_user_id ON user_api_tokens (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_api_tokens_user_id_is_revoked ON user_api_tokens (user_id, is_revoked);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_audit_events_created_at ON user_audit_events (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_audit_events_user_id ON user_audit_events (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_audit_events_user_id_event_type ON user_audit_events (user_id, event_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_auth_methods_login_method_external_subject ON user_auth_methods (login_method, external_subject);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_auth_methods_user_id ON user_auth_methods (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_auth_methods_user_id_login_method ON user_auth_methods (user_id, login_method);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_mfa_methods_user_id ON user_mfa_methods (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_mfa_methods_user_id_mfa_method ON user_mfa_methods (user_id, mfa_method);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_provisioning_sources_user_id ON user_provisioning_sources (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_recovery_codes_user_id ON user_recovery_codes (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_recovery_codes_user_id_is_used ON user_recovery_codes (user_id, is_used);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_role_assignments_user_id ON user_role_assignments (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_role_assignments_user_id_org_id ON user_role_assignments (user_id, org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_role_assignments_user_id_org_id_role_id_is_active ON user_role_assignments (user_id, org_id, role_id, is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_security_user_id ON user_security (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_sessions_jti ON user_sessions (jti);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_sessions_user_id ON user_sessions (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_user_sessions_user_id_is_revoked_expires_at ON user_sessions (user_id, is_revoked, expires_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_users_email ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_users_home_org_id ON users (home_org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_users_is_active ON users (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    CREATE INDEX ix_users_user_category ON users (user_category);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424155855_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260424155855_InitialCreate', '10.0.4');
    END IF;
END $EF$;
COMMIT;

