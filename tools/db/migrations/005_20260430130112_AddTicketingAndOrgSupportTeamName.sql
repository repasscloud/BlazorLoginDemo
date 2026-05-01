START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    ALTER TABLE organisations ADD branch_code character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    ALTER TABLE organisations ADD chain_code character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    ALTER TABLE organisations ADD chain_name character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    ALTER TABLE organisations ADD support_team_name character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE TABLE exchange_rates (
        id character varying(50) NOT NULL,
        currency_code character varying(10) NOT NULL,
        currency_name character varying(100) NOT NULL,
        rate numeric(18,8) NOT NULL,
        rate_date date NOT NULL,
        fetched_at timestamp with time zone NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_exchange_rates PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE TABLE support_tickets (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        raised_by_user_id character varying(50) NOT NULL,
        status integer NOT NULL,
        priority integer NOT NULL,
        queue integer NOT NULL,
        category integer NOT NULL,
        subject character varying(500) NOT NULL,
        description character varying(10000) NOT NULL,
        error_context text,
        git_hub_issue_number integer,
        git_hub_issue_url character varying(500),
        resolved_at timestamp with time zone,
        closed_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_support_tickets PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE TABLE ticket_comments (
        id character varying(50) NOT NULL,
        ticket_id character varying(50) NOT NULL,
        author_user_id character varying(50) NOT NULL,
        author_display_name character varying(200) NOT NULL,
        is_private boolean NOT NULL,
        body character varying(10000) NOT NULL,
        git_hub_comment_id bigint,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_ticket_comments PRIMARY KEY (id),
        CONSTRAINT fk_ticket_comments_support_tickets_ticket_id FOREIGN KEY (ticket_id) REFERENCES support_tickets (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE TABLE ticket_escalations (
        id character varying(50) NOT NULL,
        ticket_id character varying(50) NOT NULL,
        actor_user_id character varying(50) NOT NULL,
        from_queue integer NOT NULL,
        to_queue integer NOT NULL,
        is_escalation boolean NOT NULL,
        reason character varying(1000),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_ticket_escalations PRIMARY KEY (id),
        CONSTRAINT fk_ticket_escalations_support_tickets_ticket_id FOREIGN KEY (ticket_id) REFERENCES support_tickets (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_organisations_chain_code ON organisations (chain_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE UNIQUE INDEX ix_exchange_rates_currency_code ON exchange_rates (currency_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_exchange_rates_rate_date ON exchange_rates (rate_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_support_tickets_org_id ON support_tickets (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_support_tickets_org_id_status_queue ON support_tickets (org_id, status, queue);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_support_tickets_queue ON support_tickets (queue);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_support_tickets_raised_by_user_id ON support_tickets (raised_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_support_tickets_status ON support_tickets (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_ticket_comments_ticket_id ON ticket_comments (ticket_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_ticket_comments_ticket_id_is_private ON ticket_comments (ticket_id, is_private);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    CREATE INDEX ix_ticket_escalations_ticket_id ON ticket_escalations (ticket_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260430130112_AddTicketingAndOrgSupportTeamName') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260430130112_AddTicketingAndOrgSupportTeamName', '10.0.4');
    END IF;
END $EF$;
COMMIT;

