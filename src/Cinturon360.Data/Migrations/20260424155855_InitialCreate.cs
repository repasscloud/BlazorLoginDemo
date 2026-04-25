using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinturon360.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    client_secret = table.Column<string>(type: "text", nullable: true),
                    client_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consent_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    json_web_key_set = table.Column<string>(type: "text", nullable: true),
                    permissions = table.Column<string>(type: "text", nullable: true),
                    post_logout_redirect_uris = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redirect_uris = table.Column<string>(type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    descriptions = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organisations",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    org_type = table.Column<int>(type: "integer", nullable: false),
                    parent_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    logo_storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_access_overrides",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    policy_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    policy_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    granted_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_access_overrides", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_api_tokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token_class = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    token_prefix = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    scopes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_api_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_audit_events",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    actor_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_audit_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_auth_methods",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    login_method = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    external_subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    provider_config_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_auth_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_mfa_methods",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mfa_method = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    secret_or_target = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_mfa_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_provisioning_sources",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    source_identifier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    batch_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provisioned_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_provisioning_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_recovery_codes",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_recovery_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_role_assignments",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    scope_mode = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    granted_by_user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_security",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    password_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    password_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_mfa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_mfa_required = table.Column<bool>(type: "boolean", nullable: false),
                    allow_direct_login = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    lockout_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_security", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token_class = table.Column<int>(type: "integer", nullable: false),
                    jti = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_active_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_category = table.Column<int>(type: "integer", nullable: false),
                    platform_role = table.Column<int>(type: "integer", nullable: true),
                    home_org_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    is_suspended = table.Column<bool>(type: "boolean", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "UTC"),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    avatar_storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    scopes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                        column: x => x.application_id,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    authorization_id = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                        column: x => x.authorization_id,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_applications_client_id",
                table: "OpenIddictApplications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_scopes_name",
                table: "OpenIddictScopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                table: "OpenIddictTokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                table: "OpenIddictTokens",
                column: "authorization_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_reference_id",
                table: "OpenIddictTokens",
                column: "reference_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_is_active",
                table: "organisations",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_org_type",
                table: "organisations",
                column: "org_type");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_parent_org_id",
                table: "organisations",
                column: "parent_org_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_slug",
                table: "organisations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id_permission_code",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_org_id",
                table: "roles",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_org_type_is_system_role",
                table: "roles",
                columns: new[] { "org_type", "is_system_role" });

            migrationBuilder.CreateIndex(
                name: "ix_user_access_overrides_user_id",
                table: "user_access_overrides",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_access_overrides_user_id_policy_key_is_active",
                table: "user_access_overrides",
                columns: new[] { "user_id", "policy_key", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_api_tokens_token_hash",
                table: "user_api_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_api_tokens_user_id",
                table: "user_api_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_api_tokens_user_id_is_revoked",
                table: "user_api_tokens",
                columns: new[] { "user_id", "is_revoked" });

            migrationBuilder.CreateIndex(
                name: "ix_user_audit_events_created_at",
                table: "user_audit_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_audit_events_user_id",
                table: "user_audit_events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_audit_events_user_id_event_type",
                table: "user_audit_events",
                columns: new[] { "user_id", "event_type" });

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_methods_login_method_external_subject",
                table: "user_auth_methods",
                columns: new[] { "login_method", "external_subject" });

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_methods_user_id",
                table: "user_auth_methods",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_methods_user_id_login_method",
                table: "user_auth_methods",
                columns: new[] { "user_id", "login_method" });

            migrationBuilder.CreateIndex(
                name: "ix_user_mfa_methods_user_id",
                table: "user_mfa_methods",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_mfa_methods_user_id_mfa_method",
                table: "user_mfa_methods",
                columns: new[] { "user_id", "mfa_method" });

            migrationBuilder.CreateIndex(
                name: "ix_user_provisioning_sources_user_id",
                table: "user_provisioning_sources",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_recovery_codes_user_id",
                table: "user_recovery_codes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_recovery_codes_user_id_is_used",
                table: "user_recovery_codes",
                columns: new[] { "user_id", "is_used" });

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_user_id",
                table: "user_role_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_user_id_org_id",
                table: "user_role_assignments",
                columns: new[] { "user_id", "org_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assignments_user_id_org_id_role_id_is_active",
                table: "user_role_assignments",
                columns: new[] { "user_id", "org_id", "role_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_security_user_id",
                table: "user_security",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_jti",
                table: "user_sessions",
                column: "jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id_is_revoked_expires_at",
                table: "user_sessions",
                columns: new[] { "user_id", "is_revoked", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_home_org_id",
                table: "users",
                column: "home_org_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_active",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_users_user_category",
                table: "users",
                column: "user_category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenIddictScopes");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "organisations");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "user_access_overrides");

            migrationBuilder.DropTable(
                name: "user_api_tokens");

            migrationBuilder.DropTable(
                name: "user_audit_events");

            migrationBuilder.DropTable(
                name: "user_auth_methods");

            migrationBuilder.DropTable(
                name: "user_mfa_methods");

            migrationBuilder.DropTable(
                name: "user_provisioning_sources");

            migrationBuilder.DropTable(
                name: "user_recovery_codes");

            migrationBuilder.DropTable(
                name: "user_role_assignments");

            migrationBuilder.DropTable(
                name: "user_security");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");
        }
    }
}
