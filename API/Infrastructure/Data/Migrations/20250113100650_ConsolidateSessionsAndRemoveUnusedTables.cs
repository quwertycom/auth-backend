using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateSessionsAndRemoveUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tokens_account_sessions_AccountSessionId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_application_sessions_ApplicationSessionId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_user_sessions_UserSessionId",
                table: "tokens");

            migrationBuilder.DropTable(
                name: "account_sessions");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "application_sessions");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropIndex(
                name: "IX_tokens_AccountSessionId",
                table: "tokens");

            migrationBuilder.DropIndex(
                name: "IX_tokens_ApplicationSessionId",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "AccountSessionId",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "ApplicationSessionId",
                table: "tokens");

            migrationBuilder.RenameColumn(
                name: "UserSessionId",
                table: "tokens",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_UserSessionId",
                table: "tokens",
                newName: "IX_tokens_ApplicationId");

            migrationBuilder.AddColumn<long>(
                name: "SessionId",
                table: "tokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Target = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: true),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ApplicationAccountId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_sessions_application_accounts_ApplicationAccountId",
                        column: x => x.ApplicationAccountId,
                        principalTable: "application_accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_sessions_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_emails_Email",
                table: "user_emails",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_SessionId",
                table: "tokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_AccountId_ApplicationId",
                table: "sessions",
                columns: new[] { "AccountId", "ApplicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_ApplicationAccountId",
                table: "sessions",
                column: "ApplicationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_ApplicationId",
                table: "sessions",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_Target_UserId",
                table: "sessions",
                columns: new[] { "Target", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                table: "sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_applications_ApplicationId",
                table: "tokens",
                column: "ApplicationId",
                principalTable: "applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_sessions_SessionId",
                table: "tokens",
                column: "SessionId",
                principalTable: "sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tokens_applications_ApplicationId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_sessions_SessionId",
                table: "tokens");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropIndex(
                name: "IX_user_emails_Email",
                table: "user_emails");

            migrationBuilder.DropIndex(
                name: "IX_tokens_SessionId",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "tokens");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "tokens",
                newName: "UserSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_ApplicationId",
                table: "tokens",
                newName: "IX_tokens_UserSessionId");

            migrationBuilder.AddColumn<long>(
                name: "AccountSessionId",
                table: "tokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ApplicationSessionId",
                table: "tokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "account_sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
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
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    KeyHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    KeySalt = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
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
                name: "application_sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
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
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Changes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_tokens_AccountSessionId",
                table: "tokens",
                column: "AccountSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_ApplicationSessionId",
                table: "tokens",
                column: "ApplicationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_AccountId",
                table: "account_sessions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_UserId_AccountId",
                table: "account_sessions",
                columns: new[] { "UserId", "AccountId" });

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
                name: "IX_user_sessions_UserId",
                table: "user_sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_account_sessions_AccountSessionId",
                table: "tokens",
                column: "AccountSessionId",
                principalTable: "account_sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_application_sessions_ApplicationSessionId",
                table: "tokens",
                column: "ApplicationSessionId",
                principalTable: "application_sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_user_sessions_UserSessionId",
                table: "tokens",
                column: "UserSessionId",
                principalTable: "user_sessions",
                principalColumn: "Id");
        }
    }
}
