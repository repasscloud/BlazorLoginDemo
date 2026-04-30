using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketEmailTemplatesAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "email_me_updates",
                table: "support_tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "support_ticket_email_template_code",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ticket_email_templates",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    html_body = table.Column<string>(type: "text", nullable: false),
                    plain_text_body = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_email_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_email_templates_code_language_code",
                table: "ticket_email_templates",
                columns: new[] { "code", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ticket_email_templates_is_active",
                table: "ticket_email_templates",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticket_email_templates");

            migrationBuilder.DropColumn(
                name: "email_me_updates",
                table: "support_tickets");

            migrationBuilder.DropColumn(
                name: "support_ticket_email_template_code",
                table: "organisations");
        }
    }
}
