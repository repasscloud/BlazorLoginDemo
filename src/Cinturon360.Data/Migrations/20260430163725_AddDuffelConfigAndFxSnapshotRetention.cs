using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuffelConfigAndFxSnapshotRetention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_exchange_rates_currency_code",
                table: "exchange_rates");

            migrationBuilder.CreateTable(
                name: "duffel_org_configurations",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    use_sandbox = table.Column<bool>(type: "boolean", nullable: false),
                    api_base_url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    api_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    access_scope = table.Column<int>(type: "integer", nullable: false),
                    enabled_capabilities_csv = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    enabled_search_functions_csv = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    corporate_codes_csv = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    tour_codes_csv = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    updated_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_duffel_org_configurations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_currency_code",
                table: "exchange_rates",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_currency_code_fetched_at",
                table: "exchange_rates",
                columns: new[] { "currency_code", "fetched_at" });

            migrationBuilder.CreateIndex(
                name: "ix_duffel_org_configurations_is_enabled",
                table: "duffel_org_configurations",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "ix_duffel_org_configurations_org_id",
                table: "duffel_org_configurations",
                column: "org_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "duffel_org_configurations");

            migrationBuilder.DropIndex(
                name: "ix_exchange_rates_currency_code",
                table: "exchange_rates");

            migrationBuilder.DropIndex(
                name: "ix_exchange_rates_currency_code_fetched_at",
                table: "exchange_rates");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_currency_code",
                table: "exchange_rates",
                column: "currency_code",
                unique: true);
        }
    }
}
