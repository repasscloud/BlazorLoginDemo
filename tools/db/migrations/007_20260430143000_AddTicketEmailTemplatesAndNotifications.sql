START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430143000_AddTicketEmailTemplatesAndNotifications') THEN
    ALTER TABLE support_tickets ADD email_me_updates boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430143000_AddTicketEmailTemplatesAndNotifications') THEN
    ALTER TABLE organisations ADD support_ticket_email_template_code character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430143000_AddTicketEmailTemplatesAndNotifications') THEN
    CREATE TABLE ticket_email_templates (
        id character varying(50) NOT NULL,
        code character varying(200) NOT NULL,
        language_code character varying(10) NOT NULL,
        description character varying(500),
        html_body text NOT NULL,
        plain_text_body text,
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_ticket_email_templates PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430143000_AddTicketEmailTemplatesAndNotifications') THEN
    CREATE UNIQUE INDEX ix_ticket_email_templates_code_language_code ON ticket_email_templates (code, language_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430143000_AddTicketEmailTemplatesAndNotifications') THEN
    CREATE INDEX ix_ticket_email_templates_is_active ON ticket_email_templates (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430143000_AddTicketEmailTemplatesAndNotifications') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260430143000_AddTicketEmailTemplatesAndNotifications', '10.0.4');
    END IF;
END $EF$;
COMMIT;

