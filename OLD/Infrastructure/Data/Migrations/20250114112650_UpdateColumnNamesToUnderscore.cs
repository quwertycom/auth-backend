using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNamesToUnderscore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsUsed",
                table: "verification_sessions",
                newName: "is_used");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "verification_sessions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "users",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "LastLoginAt",
                table: "users",
                newName: "last_login_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "user_emails",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "user_emails",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "user_emails",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_user_emails_Email",
                table: "user_emails",
                newName: "IX_user_emails_email");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "tokens",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Target",
                table: "tokens",
                newName: "target");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "tokens",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tokens",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_Target_UserId",
                table: "tokens",
                newName: "IX_tokens_target_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_ExpiresAt",
                table: "tokens",
                newName: "IX_tokens_expires_at");

            migrationBuilder.RenameColumn(
                name: "Target",
                table: "sessions",
                newName: "target");

            migrationBuilder.RenameColumn(
                name: "LastUsedAt",
                table: "sessions",
                newName: "last_used_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "sessions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_Target_UserId",
                table: "sessions",
                newName: "IX_sessions_target_UserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "organizations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notifications",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "ReadAt",
                table: "notifications",
                newName: "read_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "notifications",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "developers",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "developers",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "developers",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_developers_Type_OrganizationId",
                table: "developers",
                newName: "IX_developers_type_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "applications",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "applications",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_applications_DeveloperId_Status",
                table: "applications",
                newName: "IX_applications_DeveloperId_status");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "accounts",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "accounts",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "state",
                table: "user_emails",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "user_emails",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            // Add the 'is_primary' column if it doesn't exist
            migrationBuilder.AddColumn<bool>(
                name: "is_primary",
                table: "user_emails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Create the unique index on 'UserId' and 'is_primary' if it doesn't exist
            migrationBuilder.CreateIndex(
                name: "IX_user_emails_UserId_is_primary",
                table: "user_emails",
                columns: new[] { "UserId", "is_primary" },
                unique: true,
                filter: "is_primary = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_emails_UserId_is_primary",
                table: "user_emails");

            migrationBuilder.DropColumn(
                name: "is_primary",
                table: "user_emails");

            migrationBuilder.RenameColumn(
                name: "is_used",
                table: "verification_sessions",
                newName: "IsUsed");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "verification_sessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "users",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "last_login_at",
                table: "users",
                newName: "LastLoginAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "user_emails",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "user_emails",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "user_emails",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_user_emails_email",
                table: "user_emails",
                newName: "IX_user_emails_Email");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "tokens",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "target",
                table: "tokens",
                newName: "Target");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "tokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tokens",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_target_UserId",
                table: "tokens",
                newName: "IX_tokens_Target_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_expires_at",
                table: "tokens",
                newName: "IX_tokens_ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "target",
                table: "sessions",
                newName: "Target");

            migrationBuilder.RenameColumn(
                name: "last_used_at",
                table: "sessions",
                newName: "LastUsedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "sessions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_target_UserId",
                table: "sessions",
                newName: "IX_sessions_Target_UserId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "organizations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "read_at",
                table: "notifications",
                newName: "ReadAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "notifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "developers",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "developers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "developers",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_developers_type_OrganizationId",
                table: "developers",
                newName: "IX_developers_Type_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "applications",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "applications",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_applications_DeveloperId_status",
                table: "applications",
                newName: "IX_applications_DeveloperId_Status");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "accounts",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "accounts",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "State",
                table: "user_emails",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "user_emails",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
