START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430133702_AddTicketAttachments') THEN
    CREATE TABLE ticket_attachments (
        id character varying(50) NOT NULL,
        ticket_id character varying(50) NOT NULL,
        uploaded_by_user_id character varying(50) NOT NULL,
        file_name character varying(500) NOT NULL,
        content_type character varying(200) NOT NULL,
        file_size bigint NOT NULL,
        content bytea NOT NULL,
        is_private boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_ticket_attachments PRIMARY KEY (id),
        CONSTRAINT fk_ticket_attachments_support_tickets_ticket_id FOREIGN KEY (ticket_id) REFERENCES support_tickets (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430133702_AddTicketAttachments') THEN
    CREATE INDEX ix_ticket_attachments_ticket_id_is_private ON ticket_attachments (ticket_id, is_private);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430133702_AddTicketAttachments') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260430133702_AddTicketAttachments', '10.0.4');
    END IF;
END $EF$;
COMMIT;

