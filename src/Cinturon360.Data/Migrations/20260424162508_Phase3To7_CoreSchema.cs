using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase3To7_CoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "currency_code",
                table: "organisations",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "external_ref",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "organisations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "primary_email",
                table: "organisations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_phone",
                table: "organisations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_zone",
                table: "organisations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "UTC");

            migrationBuilder.AddColumn<string>(
                name: "website",
                table: "organisations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "airports",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    iata_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    icao_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    city_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "approval_decisions",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    approval_request_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    approver_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    decision = table.Column<int>(type: "integer", nullable: false),
                    comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_decisions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "approval_requests",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subject_type = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requested_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_levels = table.Column<int>(type: "integer", nullable: false),
                    current_level = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "booking_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    booking_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    supplier_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    supplier_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    origin_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    destination_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    departure_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    arrival_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    flight_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cabin_class = table.Column<int>(type: "integer", nullable: true),
                    seat_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    hotel_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    check_in_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_out_date = table.Column<DateOnly>(type: "date", nullable: true),
                    gross_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    fee_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ticket_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    traveller_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    booked_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quote_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_amount_gross = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    pnr_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    supplier_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    trip_start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    trip_end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    approval_levels_required = table.Column<int>(type: "integer", nullable: false),
                    approval_levels_completed = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iata_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    iso_code2 = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    iso_code3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    issued_on = table.Column<DateOnly>(type: "date", nullable: false),
                    due_on = table.Column<DateOnly>(type: "date", nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    stripe_invoice_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    job_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "org_billing_configs",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    stripe_customer_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    billing_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    vat_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    vat_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_billing_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "org_licenses",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_type = table.Column<int>(type: "integer", nullable: false),
                    billing_cycle = table.Column<int>(type: "integer", nullable: false),
                    max_users = table.Column<int>(type: "integer", nullable: false),
                    max_bookings_per_month = table.Column<int>(type: "integer", nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    stripe_subscription_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    invoice_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    stripe_payment_intent_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    stripe_charge_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy_assignments",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    policy_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_type = table.Column<int>(type: "integer", nullable: false),
                    target_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy_rules",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    policy_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rule_type = table.Column<int>(type: "integer", nullable: false),
                    violation_action = table.Column<int>(type: "integer", nullable: false),
                    value_string = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    value_decimal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    value_int = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prepaid_balances",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    balance_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prepaid_balances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requested_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_amount_gross = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    provider_payload = table.Column<string>(type: "text", nullable: true),
                    provider_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stored_documents",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stored_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "travel_policies",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_travel_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "traveller_loyalty_programs",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    program_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    program_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    membership_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tier_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_traveller_loyalty_programs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "traveller_profiles",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    passport_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    passport_country = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    passport_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    nationality = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tsa_pre_check_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    global_entry_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    redress_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    preferred_seat_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    preferred_meal_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vip_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dedicated_consultant_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_traveller_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_addresses",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    line1 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    line2 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_emergency_contacts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_emergency_contacts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    preferred_airport_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    preferred_airline_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    preferred_hotel_chain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    preferred_car_rental_company = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notify_by_email = table.Column<bool>(type: "boolean", nullable: false),
                    notify_by_sms = table.Column<bool>(type: "boolean", nullable: false),
                    notify_booking_confirmation = table.Column<bool>(type: "boolean", nullable: false),
                    notify_approval_required = table.Column<bool>(type: "boolean", nullable: false),
                    notify_approval_decision = table.Column<bool>(type: "boolean", nullable: false),
                    notify_trip_reminders = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_preferences", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_airports_city_code",
                table: "airports",
                column: "city_code");

            migrationBuilder.CreateIndex(
                name: "ix_airports_country_code",
                table: "airports",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_airports_iata_code",
                table: "airports",
                column: "iata_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_approval_decisions_approval_request_id",
                table: "approval_decisions",
                column: "approval_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_decisions_approver_user_id",
                table: "approval_decisions",
                column: "approver_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_requests_org_id",
                table: "approval_requests",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_requests_status",
                table: "approval_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_approval_requests_subject_id",
                table: "approval_requests",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_booking_id",
                table: "booking_items",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_org_id",
                table: "bookings",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_traveller_user_id",
                table: "bookings",
                column: "traveller_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_cities_country_code",
                table: "cities",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_cities_iata_code",
                table: "cities",
                column: "iata_code");

            migrationBuilder.CreateIndex(
                name: "ix_countries_iso_code2",
                table: "countries",
                column: "iso_code2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_countries_iso_code3",
                table: "countries",
                column: "iso_code3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_org_id",
                table: "invoices",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_job_type",
                table: "jobs",
                column: "job_type");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_scheduled_at",
                table: "jobs",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_status",
                table: "jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_org_billing_configs_org_id",
                table: "org_billing_configs",
                column: "org_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_org_licenses_org_id",
                table: "org_licenses",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_org_id",
                table: "payments",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_assignments_policy_id_target_type_target_id",
                table: "policy_assignments",
                columns: new[] { "policy_id", "target_type", "target_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_rules_policy_id",
                table: "policy_rules",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_prepaid_balances_org_id",
                table: "prepaid_balances",
                column: "org_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_org_id",
                table: "quotes",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_status",
                table: "quotes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_stored_documents_org_id",
                table: "stored_documents",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_stored_documents_subject_id",
                table: "stored_documents",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_travel_policies_org_id",
                table: "travel_policies",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_traveller_loyalty_programs_user_id_program_code",
                table: "traveller_loyalty_programs",
                columns: new[] { "user_id", "program_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_traveller_profiles_user_id",
                table: "traveller_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_addresses_user_id",
                table: "user_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_emergency_contacts_user_id",
                table: "user_emergency_contacts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_preferences_user_id",
                table: "user_preferences",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airports");

            migrationBuilder.DropTable(
                name: "approval_decisions");

            migrationBuilder.DropTable(
                name: "approval_requests");

            migrationBuilder.DropTable(
                name: "booking_items");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "org_billing_configs");

            migrationBuilder.DropTable(
                name: "org_licenses");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "policy_assignments");

            migrationBuilder.DropTable(
                name: "policy_rules");

            migrationBuilder.DropTable(
                name: "prepaid_balances");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "stored_documents");

            migrationBuilder.DropTable(
                name: "travel_policies");

            migrationBuilder.DropTable(
                name: "traveller_loyalty_programs");

            migrationBuilder.DropTable(
                name: "traveller_profiles");

            migrationBuilder.DropTable(
                name: "user_addresses");

            migrationBuilder.DropTable(
                name: "user_emergency_contacts");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropColumn(
                name: "currency_code",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "external_ref",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "primary_email",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "primary_phone",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "time_zone",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "website",
                table: "organisations");
        }
    }
}
