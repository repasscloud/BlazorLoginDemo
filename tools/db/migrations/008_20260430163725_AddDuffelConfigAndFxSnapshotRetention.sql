START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    DROP INDEX ix_exchange_rates_currency_code;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    CREATE TABLE duffel_org_configurations (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        is_enabled boolean NOT NULL,
        use_sandbox boolean NOT NULL,
        api_base_url character varying(256) NOT NULL,
        api_token character varying(2000) NOT NULL,
        access_scope integer NOT NULL,
        enabled_capabilities_csv character varying(2000) NOT NULL,
        enabled_search_functions_csv character varying(4000) NOT NULL,
        corporate_codes_csv character varying(4000) NOT NULL,
        tour_codes_csv character varying(4000) NOT NULL,
        notes character varying(4000),
        updated_by_user_id character varying(50),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_duffel_org_configurations PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    CREATE INDEX ix_exchange_rates_currency_code ON exchange_rates (currency_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    CREATE INDEX ix_exchange_rates_currency_code_fetched_at ON exchange_rates (currency_code, fetched_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    CREATE INDEX ix_duffel_org_configurations_is_enabled ON duffel_org_configurations (is_enabled);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    CREATE UNIQUE INDEX ix_duffel_org_configurations_org_id ON duffel_org_configurations (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430163725_AddDuffelConfigAndFxSnapshotRetention') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260430163725_AddDuffelConfigAndFxSnapshotRetention', '10.0.4');
    END IF;
END $EF$;
COMMIT;

