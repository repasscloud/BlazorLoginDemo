using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderNeutralBillingArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "billing_relationships",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seller_organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    buyer_organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_license_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_mode = table.Column<int>(type: "integer", nullable: false),
                    billing_frequency = table.Column<int>(type: "integer", nullable: false),
                    collection_mode = table.Column<int>(type: "integer", nullable: false),
                    charge_treatment = table.Column<int>(type: "integer", nullable: false),
                    generate_invoices = table.Column<bool>(type: "boolean", nullable: false),
                    auto_collect_payment = table.Column<bool>(type: "boolean", nullable: false),
                    requires_payment_setup = table.Column<bool>(type: "boolean", nullable: false),
                    is_billing_setup_complete = table.Column<bool>(type: "boolean", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    payment_terms_days = table.Column<int>(type: "integer", nullable: false),
                    credit_limit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    max_single_booking_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    block_bookings_when_overdue = table.Column<bool>(type: "boolean", nullable: false),
                    require_payment_before_ticketing = table.Column<bool>(type: "boolean", nullable: false),
                    commercial_risk_owner = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_relationships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organisation_billing_profiles",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seller_organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    default_provider_customer_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    default_provider_payment_method_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    default_billing_mode = table.Column<int>(type: "integer", nullable: false),
                    default_billing_frequency = table.Column<int>(type: "integer", nullable: false),
                    default_collection_mode = table.Column<int>(type: "integer", nullable: false),
                    default_charge_treatment = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    is_billing_setup_complete = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_billing_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_provider_connections",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owner_organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_type = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_live_mode = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    secret_bundle_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    provider_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    provider_account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    webhook_endpoint_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    webhook_secret_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    usage_scope = table.Column<int>(type: "integer", nullable: false),
                    allow_child_org_billing = table.Column<bool>(type: "boolean", nullable: false),
                    allow_client_checkout = table.Column<bool>(type: "boolean", nullable: false),
                    allow_monthly_invoice_collection = table.Column<bool>(type: "boolean", nullable: false),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_rotated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_webhook_received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_provider_connections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_customers",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    buyer_organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_customer_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    has_usable_payment_method = table.Column<bool>(type: "boolean", nullable: false),
                    setup_completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_payment_succeeded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_payment_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_payment_methods",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_customer_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_type = table.Column<int>(type: "integer", nullable: false),
                    provider_payment_method_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    provider_setup_intent_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    purpose = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    brand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    expiry_month = table.Column<int>(type: "integer", nullable: true),
                    expiry_year = table.Column<int>(type: "integer", nullable: true),
                    cardholder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fingerprint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    billing_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_default_for_account_fees = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_webhook_events",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_type = table.Column<int>(type: "integer", nullable: false),
                    provider_event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    raw_payload_json = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_webhook_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "travel_policy_billing_rules",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    travel_policy_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resolution_mode = table.Column<int>(type: "integer", nullable: false),
                    provider_payment_method_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_customer_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_mode_override = table.Column<int>(type: "integer", nullable: true),
                    collection_mode_override = table.Column<int>(type: "integer", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_travel_policy_billing_rules", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_billing_relationships_seller_organisation_id_buyer_organisa",
                table: "billing_relationships",
                columns: new[] { "seller_organisation_id", "buyer_organisation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_billing_profiles_organisation_id",
                table: "organisation_billing_profiles",
                column: "organisation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_provider_connections_owner_organisation_id_provider",
                table: "payment_provider_connections",
                columns: new[] { "owner_organisation_id", "provider_type", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "ix_provider_customers_payment_provider_connection_id_buyer_org",
                table: "provider_customers",
                columns: new[] { "payment_provider_connection_id", "buyer_organisation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_customers_payment_provider_connection_id_provider_",
                table: "provider_customers",
                columns: new[] { "payment_provider_connection_id", "provider_customer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_payment_methods_provider_customer_id_provider_paym",
                table: "provider_payment_methods",
                columns: new[] { "provider_customer_id", "provider_payment_method_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_webhook_events_payment_provider_connection_id_prov",
                table: "provider_webhook_events",
                columns: new[] { "payment_provider_connection_id", "provider_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_travel_policy_billing_rules_organisation_id_travel_policy_i",
                table: "travel_policy_billing_rules",
                columns: new[] { "organisation_id", "travel_policy_id", "is_enabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "billing_relationships");

            migrationBuilder.DropTable(
                name: "organisation_billing_profiles");

            migrationBuilder.DropTable(
                name: "payment_provider_connections");

            migrationBuilder.DropTable(
                name: "provider_customers");

            migrationBuilder.DropTable(
                name: "provider_payment_methods");

            migrationBuilder.DropTable(
                name: "provider_webhook_events");

            migrationBuilder.DropTable(
                name: "travel_policy_billing_rules");
        }
    }
}
