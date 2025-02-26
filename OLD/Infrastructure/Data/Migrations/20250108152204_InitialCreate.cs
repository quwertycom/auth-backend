using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordSalt = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "developers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_developers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_developers_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "organization_roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_roles_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccountName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_personal = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_accounts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Changes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_emails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_emails_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    RedirectUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeveloperId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_applications_developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_developer_authorizations",
                columns: table => new
                {
                    AuthorizedAccountsId = table.Column<long>(type: "bigint", nullable: false),
                    AuthorizedDevelopersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_developer_authorizations", x => new { x.AuthorizedAccountsId, x.AuthorizedDevelopersId });
                    table.ForeignKey(
                        name: "FK_account_developer_authorizations_accounts_AuthorizedAccount~",
                        column: x => x.AuthorizedAccountsId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_developer_authorizations_developers_AuthorizedDevel~",
                        column: x => x.AuthorizedDevelopersId,
                        principalTable: "developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_organization_roles",
                columns: table => new
                {
                    MembersId = table.Column<long>(type: "bigint", nullable: false),
                    RolesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_organization_roles", x => new { x.MembersId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_account_organization_roles_accounts_MembersId",
                        column: x => x.MembersId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_organization_roles_organization_roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "organization_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_sessions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    KeySalt = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_api_keys_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_accounts_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_accounts_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: true),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReadAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_sessions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_sessions_application_accounts_ApplicationAccoun~",
                        column: x => x.ApplicationAccountId,
                        principalTable: "application_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_sessions_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TokenString = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Target = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationAccountId = table.Column<long>(type: "bigint", nullable: true),
                    AccountId = table.Column<long>(type: "bigint", nullable: true),
                    UserSessionId = table.Column<long>(type: "bigint", nullable: true),
                    AccountSessionId = table.Column<long>(type: "bigint", nullable: true),
                    ApplicationSessionId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tokens_account_sessions_AccountSessionId",
                        column: x => x.AccountSessionId,
                        principalTable: "account_sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tokens_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tokens_application_accounts_ApplicationAccountId",
                        column: x => x.ApplicationAccountId,
                        principalTable: "application_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tokens_application_sessions_ApplicationSessionId",
                        column: x => x.ApplicationSessionId,
                        principalTable: "application_sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tokens_user_sessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "user_sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_developer_authorizations_AuthorizedDevelopersId",
                table: "account_developer_authorizations",
                column: "AuthorizedDevelopersId");

            migrationBuilder.CreateIndex(
                name: "IX_account_organization_roles_RolesId",
                table: "account_organization_roles",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_AccountId",
                table: "account_sessions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_UserId_AccountId",
                table: "account_sessions",
                columns: new[] { "UserId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_OnePersonalAccountPerUser",
                table: "accounts",
                columns: new[] { "UserId", "is_personal" },
                unique: true,
                filter: "is_personal = true");

            migrationBuilder.CreateIndex(
                name: "IX_OnePersonalAccountPerUser1",
                table: "accounts",
                columns: new[] { "UserId", "is_personal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_AccountName",
                table: "accounts",
                column: "AccountName");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_OrganizationId",
                table: "accounts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_ApplicationId",
                table: "api_keys",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_ExpiresAt",
                table: "api_keys",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_Status",
                table: "api_keys",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_application_accounts_AccountId",
                table: "application_accounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_application_accounts_ApplicationId",
                table: "application_accounts",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_application_sessions_AccountId",
                table: "application_sessions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_application_sessions_ApplicationAccountId",
                table: "application_sessions",
                column: "ApplicationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_application_sessions_ApplicationId",
                table: "application_sessions",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_application_sessions_UserId_AccountId_ApplicationId",
                table: "application_sessions",
                columns: new[] { "UserId", "AccountId", "ApplicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_applications_DeveloperId_Status",
                table: "applications",
                columns: new[] { "DeveloperId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_applications_Name",
                table: "applications",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CreatedAt",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OnePersonalDeveloperPerAccount",
                table: "developers",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_developers_Name",
                table: "developers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_developers_OrganizationId",
                table: "developers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_developers_Type_OrganizationId",
                table: "developers",
                columns: new[] { "Type", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_AccountId_ApplicationId",
                table: "notifications",
                columns: new[] { "AccountId", "ApplicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ApplicationId",
                table: "notifications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_IsRead",
                table: "notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_organization_roles_OrganizationId",
                table: "organization_roles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_Name",
                table: "organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_AccountId",
                table: "tokens",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_AccountSessionId",
                table: "tokens",
                column: "AccountSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_ApplicationAccountId",
                table: "tokens",
                column: "ApplicationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_ApplicationSessionId",
                table: "tokens",
                column: "ApplicationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_ExpiresAt",
                table: "tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_Target_UserId",
                table: "tokens",
                columns: new[] { "Target", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_tokens_TokenString",
                table: "tokens",
                column: "TokenString",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_UserId",
                table: "tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_UserSessionId",
                table: "tokens",
                column: "UserSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_emails_UserId",
                table: "user_emails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId",
                table: "user_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_PhoneNumber",
                table: "users",
                column: "PhoneNumber",
                unique: true,
                filter: "\"PhoneNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_login_name",
                table: "users",
                column: "login_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_developer_authorizations");

            migrationBuilder.DropTable(
                name: "account_organization_roles");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "user_emails");

            migrationBuilder.DropTable(
                name: "organization_roles");

            migrationBuilder.DropTable(
                name: "account_sessions");

            migrationBuilder.DropTable(
                name: "application_sessions");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "application_accounts");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "developers");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
