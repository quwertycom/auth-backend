using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class updatedMigrationsAndUniteIdNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_developer_authorizations_accounts_authorized_accoun~",
                table: "account_developer_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_account_developer_authorizations_developers_authorized_deve~",
                table: "account_developer_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_account_organization_roles_accounts_member_id",
                table: "account_organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_accounts_account_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_applications_application_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_accounts_account_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_applications_application_id",
                table: "sessions");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "user_email_id",
                table: "user_emails",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "member_id",
                table: "account_organization_roles",
                newName: "account_id");

            migrationBuilder.RenameColumn(
                name: "authorized_developer_id",
                table: "account_developer_authorizations",
                newName: "developer_id");

            migrationBuilder.RenameColumn(
                name: "authorized_account_id",
                table: "account_developer_authorizations",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_account_developer_authorizations_authorized_developer_id",
                table: "account_developer_authorizations",
                newName: "IX_account_developer_authorizations_developer_id");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "verification_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_account_developer_authorizations_accounts_account_id",
                table: "account_developer_authorizations",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_developer_authorizations_developers_developer_id",
                table: "account_developer_authorizations",
                column: "developer_id",
                principalTable: "developers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_organization_roles_accounts_account_id",
                table: "account_organization_roles",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_accounts_account_id",
                table: "notifications",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_applications_application_id",
                table: "notifications",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_accounts_account_id",
                table: "sessions",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_applications_application_id",
                table: "sessions",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_developer_authorizations_accounts_account_id",
                table: "account_developer_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_account_developer_authorizations_developers_developer_id",
                table: "account_developer_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_account_organization_roles_accounts_account_id",
                table: "account_organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_accounts_account_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_applications_application_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_accounts_account_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_applications_application_id",
                table: "sessions");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "user_emails",
                newName: "user_email_id");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "account_organization_roles",
                newName: "member_id");

            migrationBuilder.RenameColumn(
                name: "developer_id",
                table: "account_developer_authorizations",
                newName: "authorized_developer_id");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "account_developer_authorizations",
                newName: "authorized_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_account_developer_authorizations_developer_id",
                table: "account_developer_authorizations",
                newName: "IX_account_developer_authorizations_authorized_developer_id");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "verification_sessions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_account_developer_authorizations_accounts_authorized_accoun~",
                table: "account_developer_authorizations",
                column: "authorized_account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_developer_authorizations_developers_authorized_deve~",
                table: "account_developer_authorizations",
                column: "authorized_developer_id",
                principalTable: "developers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_organization_roles_accounts_member_id",
                table: "account_organization_roles",
                column: "member_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_accounts_account_id",
                table: "notifications",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_applications_application_id",
                table: "notifications",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_accounts_account_id",
                table: "sessions",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_applications_application_id",
                table: "sessions",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id");
        }
    }
}
