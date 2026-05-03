using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBillingModelToLicenseAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "travel_policy_billing_rules");

            migrationBuilder.AddColumn<bool>(
                name: "is_primary",
                table: "payment_provider_connections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "webhook_provisioned_at_utc",
                table: "payment_provider_connections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "billing_accounts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owner_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_model = table.Column<int>(type: "integer", nullable: false),
                    account_status = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AUD"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "billing_invoice_lines",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    billing_invoice_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    unit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    related_entity_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_invoice_lines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "billing_invoices",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seller_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    buyer_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AUD"),
                    issued_on = table.Column<DateOnly>(type: "date", nullable: false),
                    due_on = table.Column<DateOnly>(type: "date", nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "billing_ledger_entries",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    billing_account_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AUD"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    entry_date_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_ledger_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_invoice_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_attempt_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    entry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "journal_lines",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    journal_entry_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AUD"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_lines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_agreement_entitlements",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    value_kind = table.Column<int>(type: "integer", nullable: false),
                    boolean_value = table.Column<bool>(type: "boolean", nullable: true),
                    numeric_value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    text_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_unlimited = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_agreement_entitlements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_agreements",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seller_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    buyer_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    billing_model = table.Column<int>(type: "integer", nullable: false),
                    billing_period = table.Column<int>(type: "integer", nullable: false),
                    collection_mode = table.Column<int>(type: "integer", nullable: false),
                    payment_terms_days = table.Column<int>(type: "integer", nullable: false),
                    credit_limit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AUD"),
                    require_payment_before_ticketing = table.Column<bool>(type: "boolean", nullable: false),
                    access_package_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    previous_license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    superseded_by_license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_agreements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_collection_policies",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    license_agreement_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_terms_days = table.Column<int>(type: "integer", nullable: false),
                    grace_period_days = table.Column<int>(type: "integer", nullable: false),
                    block_bookings_when_overdue = table.Column<bool>(type: "boolean", nullable: false),
                    require_payment_before_ticketing = table.Column<bool>(type: "boolean", nullable: false),
                    action_after_grace = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_collection_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_attempts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    billing_invoice_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_account_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_payment_intent_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "AUD"),
                    status = table.Column<int>(type: "integer", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy_billing_rules",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    policy_type = table.Column<int>(type: "integer", nullable: false),
                    policy_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resolution_mode = table.Column<int>(type: "integer", nullable: false),
                    provider_payment_method_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_customer_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_mode_override = table.Column<int>(type: "integer", nullable: true),
                    collection_mode_override = table.Column<int>(type: "integer", nullable: true),
                    require_payment_before_execution = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_billing_rules", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_payment_provider_connection_primary",
                table: "payment_provider_connections",
                columns: new[] { "owner_organisation_id", "provider_type", "is_live_mode", "usage_scope" },
                unique: true,
                filter: "is_primary = true AND is_enabled = true AND status = 2");

            migrationBuilder.CreateIndex(
                name: "ix_billing_accounts_owner_org_id",
                table: "billing_accounts",
                column: "owner_org_id");

            migrationBuilder.CreateIndex(
                name: "ix_billing_invoice_lines_billing_invoice_id",
                table: "billing_invoice_lines",
                column: "billing_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_billing_invoices_invoice_number",
                table: "billing_invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_billing_invoices_seller_org_id_buyer_org_id_status",
                table: "billing_invoices",
                columns: new[] { "seller_org_id", "buyer_org_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_billing_ledger_entries_billing_account_id",
                table: "billing_ledger_entries",
                column: "billing_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_billing_invoice_id",
                table: "journal_entries",
                column: "billing_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_lines_journal_entry_id",
                table: "journal_lines",
                column: "journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_license_agreement_entitlements_license_agreement_id_type_is",
                table: "license_agreement_entitlements",
                columns: new[] { "license_agreement_id", "type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_license_agreements_seller_org_id_buyer_org_id_status",
                table: "license_agreements",
                columns: new[] { "seller_org_id", "buyer_org_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_license_collection_policies_license_agreement_id",
                table: "license_collection_policies",
                column: "license_agreement_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_attempts_billing_invoice_id",
                table: "payment_attempts",
                column: "billing_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_billing_rules_organisation_id_policy_type_policy_id_",
                table: "policy_billing_rules",
                columns: new[] { "organisation_id", "policy_type", "policy_id", "is_enabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "billing_accounts");

            migrationBuilder.DropTable(
                name: "billing_invoice_lines");

            migrationBuilder.DropTable(
                name: "billing_invoices");

            migrationBuilder.DropTable(
                name: "billing_ledger_entries");

            migrationBuilder.DropTable(
                name: "journal_entries");

            migrationBuilder.DropTable(
                name: "journal_lines");

            migrationBuilder.DropTable(
                name: "license_agreement_entitlements");

            migrationBuilder.DropTable(
                name: "license_agreements");

            migrationBuilder.DropTable(
                name: "license_collection_policies");

            migrationBuilder.DropTable(
                name: "payment_attempts");

            migrationBuilder.DropTable(
                name: "policy_billing_rules");

            migrationBuilder.DropIndex(
                name: "ux_payment_provider_connection_primary",
                table: "payment_provider_connections");

            migrationBuilder.DropColumn(
                name: "is_primary",
                table: "payment_provider_connections");

            migrationBuilder.DropColumn(
                name: "webhook_provisioned_at_utc",
                table: "payment_provider_connections");

            migrationBuilder.CreateTable(
                name: "travel_policy_billing_rules",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    billing_mode_override = table.Column<int>(type: "integer", nullable: true),
                    collection_mode_override = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    organisation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_provider_connection_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_customer_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_payment_method_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    resolution_mode = table.Column<int>(type: "integer", nullable: false),
                    travel_policy_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_travel_policy_billing_rules", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_travel_policy_billing_rules_organisation_id_travel_policy_i",
                table: "travel_policy_billing_rules",
                columns: new[] { "organisation_id", "travel_policy_id", "is_enabled" });
        }
    }
}
