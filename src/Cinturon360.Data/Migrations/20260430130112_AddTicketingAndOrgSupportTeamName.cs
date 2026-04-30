using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketingAndOrgSupportTeamName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "branch_code",
                table: "organisations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chain_code",
                table: "organisations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chain_name",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "support_team_name",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    currency_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    rate_date = table.Column<DateOnly>(type: "date", nullable: false),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "support_tickets",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raised_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    queue = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    error_context = table.Column<string>(type: "text", nullable: true),
                    git_hub_issue_number = table.Column<int>(type: "integer", nullable: true),
                    git_hub_issue_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_support_tickets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticket_comments",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ticket_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    author_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    author_display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_private = table.Column<bool>(type: "boolean", nullable: false),
                    body = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    git_hub_comment_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_comments_support_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "support_tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_escalations",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ticket_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    actor_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    from_queue = table.Column<int>(type: "integer", nullable: false),
                    to_queue = table.Column<int>(type: "integer", nullable: false),
                    is_escalation = table.Column<bool>(type: "boolean", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_escalations", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_escalations_support_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "support_tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organisations_chain_code",
                table: "organisations",
                column: "chain_code");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_currency_code",
                table: "exchange_rates",
                column: "currency_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_rate_date",
                table: "exchange_rates",
                column: "rate_date");

            migrationBuilder.CreateIndex(
                name: "ix_support_tickets_org_id",
                table: "support_tickets",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_support_tickets_org_id_status_queue",
                table: "support_tickets",
                columns: new[] { "org_id", "status", "queue" });

            migrationBuilder.CreateIndex(
                name: "ix_support_tickets_queue",
                table: "support_tickets",
                column: "queue");

            migrationBuilder.CreateIndex(
                name: "ix_support_tickets_raised_by_user_id",
                table: "support_tickets",
                column: "raised_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_support_tickets_status",
                table: "support_tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comments_ticket_id",
                table: "ticket_comments",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comments_ticket_id_is_private",
                table: "ticket_comments",
                columns: new[] { "ticket_id", "is_private" });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_escalations_ticket_id",
                table: "ticket_escalations",
                column: "ticket_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "ticket_comments");

            migrationBuilder.DropTable(
                name: "ticket_escalations");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropIndex(
                name: "ix_organisations_chain_code",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "branch_code",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "chain_code",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "chain_name",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "support_team_name",
                table: "organisations");
        }
    }
}
