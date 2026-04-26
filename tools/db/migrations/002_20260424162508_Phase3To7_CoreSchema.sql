START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD currency_code character varying(3) NOT NULL DEFAULT 'USD';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD external_ref character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD language_code character varying(10) NOT NULL DEFAULT 'en';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD primary_email character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD primary_phone character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD time_zone character varying(100) NOT NULL DEFAULT 'UTC';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    ALTER TABLE organisations ADD website character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE airports (
        id character varying(50) NOT NULL,
        iata_code character varying(4) NOT NULL,
        icao_code character varying(5),
        name character varying(300) NOT NULL,
        city_code character varying(10) NOT NULL,
        country_code character varying(3) NOT NULL,
        time_zone character varying(100),
        latitude numeric(9,6),
        longitude numeric(9,6),
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_airports PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE approval_decisions (
        id character varying(50) NOT NULL,
        approval_request_id character varying(50) NOT NULL,
        level integer NOT NULL,
        approver_user_id character varying(50) NOT NULL,
        decision integer NOT NULL,
        comments character varying(2000),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_approval_decisions PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE approval_requests (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        subject_type integer NOT NULL,
        subject_id character varying(50) NOT NULL,
        requested_by_user_id character varying(50) NOT NULL,
        status integer NOT NULL,
        total_levels integer NOT NULL,
        current_level integer NOT NULL,
        notes character varying(2000),
        resolved_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_approval_requests PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE booking_items (
        id character varying(50) NOT NULL,
        booking_id character varying(50) NOT NULL,
        item_type integer NOT NULL,
        sort_order integer NOT NULL,
        supplier_code character varying(50),
        supplier_ref character varying(100),
        origin_code character varying(10),
        destination_code character varying(10),
        departure_at timestamp with time zone,
        arrival_at timestamp with time zone,
        flight_number character varying(20),
        cabin_class integer,
        seat_number character varying(10),
        hotel_name character varying(300),
        check_in_date date,
        check_out_date date,
        gross_amount numeric(18,4) NOT NULL,
        tax_amount numeric(18,4),
        fee_amount numeric(18,4),
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        ticket_number character varying(50),
        is_cancelled boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_booking_items PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE bookings (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        traveller_user_id character varying(50) NOT NULL,
        booked_by_user_id character varying(50),
        quote_id character varying(50),
        status integer NOT NULL,
        total_amount_gross numeric(18,4) NOT NULL,
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        pnr_code character varying(20),
        supplier_ref character varying(100),
        external_ref character varying(200),
        trip_start_date timestamp with time zone,
        trip_end_date timestamp with time zone,
        confirmed_at timestamp with time zone,
        cancelled_at timestamp with time zone,
        cancellation_reason character varying(1000),
        approval_levels_required integer NOT NULL,
        approval_levels_completed integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_bookings PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE cities (
        id character varying(50) NOT NULL,
        name character varying(200) NOT NULL,
        iata_code character varying(10),
        country_code character varying(3) NOT NULL,
        time_zone character varying(100),
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_cities PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE countries (
        id character varying(50) NOT NULL,
        iso_code2 character varying(2) NOT NULL,
        iso_code3 character varying(3) NOT NULL,
        name character varying(200) NOT NULL,
        phone_prefix character varying(10),
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_countries PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE invoices (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        invoice_number character varying(50) NOT NULL,
        status integer NOT NULL,
        subtotal_amount numeric(18,4) NOT NULL,
        tax_amount numeric(18,4) NOT NULL,
        total_amount numeric(18,4) NOT NULL,
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        issued_on date NOT NULL,
        due_on date NOT NULL,
        paid_at timestamp with time zone,
        stripe_invoice_id character varying(200),
        notes character varying(2000),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_invoices PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE jobs (
        id character varying(50) NOT NULL,
        job_type integer NOT NULL,
        status integer NOT NULL,
        org_id character varying(50),
        user_id character varying(50),
        payload text,
        attempts integer NOT NULL,
        max_attempts integer NOT NULL,
        scheduled_at timestamp with time zone,
        started_at timestamp with time zone,
        completed_at timestamp with time zone,
        next_retry_at timestamp with time zone,
        error_message character varying(4000),
        trace_id character varying(100),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_jobs PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE org_billing_configs (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        stripe_customer_id character varying(200),
        billing_email character varying(255),
        billing_name character varying(200),
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        vat_exempt boolean NOT NULL,
        vat_number character varying(50),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_org_billing_configs PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE org_licenses (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        license_type integer NOT NULL,
        billing_cycle integer NOT NULL,
        max_users integer NOT NULL,
        max_bookings_per_month integer NOT NULL,
        starts_on date NOT NULL,
        expires_on date,
        is_active boolean NOT NULL,
        stripe_subscription_id character varying(200),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_org_licenses PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE payments (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        invoice_id character varying(50),
        status integer NOT NULL,
        method integer NOT NULL,
        amount numeric(18,4) NOT NULL,
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        stripe_payment_intent_id character varying(200),
        stripe_charge_id character varying(200),
        processed_at timestamp with time zone,
        failure_reason character varying(1000),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_payments PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE policy_assignments (
        id character varying(50) NOT NULL,
        policy_id character varying(50) NOT NULL,
        target_type integer NOT NULL,
        target_id character varying(50) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_policy_assignments PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE policy_rules (
        id character varying(50) NOT NULL,
        policy_id character varying(50) NOT NULL,
        rule_type integer NOT NULL,
        violation_action integer NOT NULL,
        value_string character varying(500),
        value_decimal numeric(18,4),
        value_int integer,
        description character varying(500),
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_policy_rules PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE prepaid_balances (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        balance_amount numeric(18,4) NOT NULL,
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_prepaid_balances PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE quotes (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        requested_by_user_id character varying(50) NOT NULL,
        status integer NOT NULL,
        total_amount_gross numeric(18,4) NOT NULL,
        currency_code character varying(3) NOT NULL DEFAULT 'USD',
        expires_at timestamp with time zone NOT NULL,
        provider_payload text,
        provider_ref character varying(200),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_quotes PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE stored_documents (
        id character varying(50) NOT NULL,
        document_type integer NOT NULL,
        org_id character varying(50),
        user_id character varying(50),
        subject_id character varying(50),
        file_name character varying(500) NOT NULL,
        storage_key character varying(1000) NOT NULL,
        content_type character varying(200),
        file_size_bytes bigint NOT NULL,
        is_public boolean NOT NULL,
        expires_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_stored_documents PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE travel_policies (
        id character varying(50) NOT NULL,
        org_id character varying(50) NOT NULL,
        name character varying(200) NOT NULL,
        description character varying(1000),
        is_default boolean NOT NULL,
        is_active boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by_user_id text,
        CONSTRAINT pk_travel_policies PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE traveller_loyalty_programs (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        program_code character varying(50) NOT NULL,
        program_name character varying(200) NOT NULL,
        membership_number character varying(100) NOT NULL,
        tier_name character varying(100),
        expiry_date date,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_traveller_loyalty_programs PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE traveller_profiles (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        passport_number character varying(50),
        passport_country character varying(3),
        passport_expiry date,
        nationality character varying(3),
        date_of_birth date,
        gender character varying(20),
        tsa_pre_check_number character varying(50),
        global_entry_number character varying(50),
        redress_number character varying(50),
        preferred_seat_type character varying(50),
        preferred_meal_type character varying(50),
        vip_level character varying(50),
        dedicated_consultant_user_id character varying(50),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_traveller_profiles PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE user_addresses (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        address_type character varying(50) NOT NULL,
        line1 character varying(300) NOT NULL,
        line2 character varying(300),
        city character varying(100) NOT NULL,
        state_province character varying(100),
        postal_code character varying(20) NOT NULL,
        country_code character varying(3) NOT NULL,
        is_default boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_addresses PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE user_emergency_contacts (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        name character varying(200) NOT NULL,
        relationship character varying(100) NOT NULL,
        phone character varying(50) NOT NULL,
        email character varying(255),
        is_primary boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_emergency_contacts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE TABLE user_preferences (
        id character varying(50) NOT NULL,
        user_id character varying(50) NOT NULL,
        preferred_airport_code character varying(10),
        preferred_airline_code character varying(10),
        preferred_hotel_chain character varying(100),
        preferred_car_rental_company character varying(100),
        notify_by_email boolean NOT NULL,
        notify_by_sms boolean NOT NULL,
        notify_booking_confirmation boolean NOT NULL,
        notify_approval_required boolean NOT NULL,
        notify_approval_decision boolean NOT NULL,
        notify_trip_reminders boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_preferences PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_airports_city_code ON airports (city_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_airports_country_code ON airports (country_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_airports_iata_code ON airports (iata_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_approval_decisions_approval_request_id ON approval_decisions (approval_request_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_approval_decisions_approver_user_id ON approval_decisions (approver_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_approval_requests_org_id ON approval_requests (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_approval_requests_status ON approval_requests (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_approval_requests_subject_id ON approval_requests (subject_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_booking_items_booking_id ON booking_items (booking_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_bookings_org_id ON bookings (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_bookings_status ON bookings (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_bookings_traveller_user_id ON bookings (traveller_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_cities_country_code ON cities (country_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_cities_iata_code ON cities (iata_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_countries_iso_code2 ON countries (iso_code2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_countries_iso_code3 ON countries (iso_code3);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_invoices_invoice_number ON invoices (invoice_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_invoices_org_id ON invoices (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_jobs_job_type ON jobs (job_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_jobs_scheduled_at ON jobs (scheduled_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_jobs_status ON jobs (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_org_billing_configs_org_id ON org_billing_configs (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_org_licenses_org_id ON org_licenses (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_payments_invoice_id ON payments (invoice_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_payments_org_id ON payments (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_policy_assignments_policy_id_target_type_target_id ON policy_assignments (policy_id, target_type, target_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_policy_rules_policy_id ON policy_rules (policy_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_prepaid_balances_org_id ON prepaid_balances (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_quotes_org_id ON quotes (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_quotes_status ON quotes (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_stored_documents_org_id ON stored_documents (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_stored_documents_subject_id ON stored_documents (subject_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_travel_policies_org_id ON travel_policies (org_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_traveller_loyalty_programs_user_id_program_code ON traveller_loyalty_programs (user_id, program_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_traveller_profiles_user_id ON traveller_profiles (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_user_addresses_user_id ON user_addresses (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE INDEX ix_user_emergency_contacts_user_id ON user_emergency_contacts (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    CREATE UNIQUE INDEX ix_user_preferences_user_id ON user_preferences (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260424162508_Phase3To7_CoreSchema') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260424162508_Phase3To7_CoreSchema', '10.0.4');
    END IF;
END $EF$;
COMMIT;

