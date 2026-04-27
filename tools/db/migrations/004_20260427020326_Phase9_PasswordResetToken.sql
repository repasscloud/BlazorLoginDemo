START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260427020326_Phase9_PasswordResetToken') THEN
    ALTER TABLE user_security ADD password_reset_expires_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260427020326_Phase9_PasswordResetToken') THEN
    ALTER TABLE user_security ADD password_reset_token_hash character varying(128);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260427020326_Phase9_PasswordResetToken') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260427020326_Phase9_PasswordResetToken', '10.0.4');
    END IF;
END $EF$;
COMMIT;

