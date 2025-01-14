using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRemainingColumnNamesToUnderscoreType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_developer_authorizations_accounts_AuthorizedAccount~",
                table: "account_developer_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_account_developer_authorizations_developers_AuthorizedDevel~",
                table: "account_developer_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_account_organization_roles_accounts_MembersId",
                table: "account_organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_account_organization_roles_organization_roles_RolesId",
                table: "account_organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_application_accounts_ApplicationAccountId",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_emails_users_UserId",
                table: "user_emails");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "users",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "users",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordSalt",
                table: "users",
                newName: "password_salt");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "BirthDate",
                table: "users",
                newName: "birth_date");

            migrationBuilder.RenameIndex(
                name: "IX_users_PhoneNumber",
                table: "users",
                newName: "IX_users_phone_number");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_emails",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_emails_UserId_is_primary",
                table: "user_emails",
                newName: "IX_user_emails_user_id_is_primary");

            migrationBuilder.RenameIndex(
                name: "IX_user_emails_UserId",
                table: "user_emails",
                newName: "IX_user_emails_user_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationAccountId",
                table: "sessions",
                newName: "application_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_ApplicationAccountId",
                table: "sessions",
                newName: "IX_sessions_application_account_id");

            migrationBuilder.RenameColumn(
                name: "RolesId",
                table: "account_organization_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "MembersId",
                table: "account_organization_roles",
                newName: "member_id");

            migrationBuilder.RenameIndex(
                name: "IX_account_organization_roles_RolesId",
                table: "account_organization_roles",
                newName: "IX_account_organization_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "AuthorizedDevelopersId",
                table: "account_developer_authorizations",
                newName: "authorized_developer_id");

            migrationBuilder.RenameColumn(
                name: "AuthorizedAccountsId",
                table: "account_developer_authorizations",
                newName: "authorized_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_account_developer_authorizations_AuthorizedDevelopersId",
                table: "account_developer_authorizations",
                newName: "IX_account_developer_authorizations_authorized_developer_id");

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
                name: "FK_account_organization_roles_organization_roles_role_id",
                table: "account_organization_roles",
                column: "role_id",
                principalTable: "organization_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_application_accounts_application_account_id",
                table: "sessions",
                column: "application_account_id",
                principalTable: "application_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_emails_users_user_id",
                table: "user_emails",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_account_organization_roles_organization_roles_role_id",
                table: "account_organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_application_accounts_application_account_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_emails_users_user_id",
                table: "user_emails");

            migrationBuilder.RenameColumn(
                name: "gender",
                table: "users",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "users",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_salt",
                table: "users",
                newName: "PasswordSalt");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "birth_date",
                table: "users",
                newName: "BirthDate");

            migrationBuilder.RenameIndex(
                name: "IX_users_phone_number",
                table: "users",
                newName: "IX_users_PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_emails",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_user_emails_user_id_is_primary",
                table: "user_emails",
                newName: "IX_user_emails_UserId_is_primary");

            migrationBuilder.RenameIndex(
                name: "IX_user_emails_user_id",
                table: "user_emails",
                newName: "IX_user_emails_UserId");

            migrationBuilder.RenameColumn(
                name: "application_account_id",
                table: "sessions",
                newName: "ApplicationAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_application_account_id",
                table: "sessions",
                newName: "IX_sessions_ApplicationAccountId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "account_organization_roles",
                newName: "RolesId");

            migrationBuilder.RenameColumn(
                name: "member_id",
                table: "account_organization_roles",
                newName: "MembersId");

            migrationBuilder.RenameIndex(
                name: "IX_account_organization_roles_role_id",
                table: "account_organization_roles",
                newName: "IX_account_organization_roles_RolesId");

            migrationBuilder.RenameColumn(
                name: "authorized_developer_id",
                table: "account_developer_authorizations",
                newName: "AuthorizedDevelopersId");

            migrationBuilder.RenameColumn(
                name: "authorized_account_id",
                table: "account_developer_authorizations",
                newName: "AuthorizedAccountsId");

            migrationBuilder.RenameIndex(
                name: "IX_account_developer_authorizations_authorized_developer_id",
                table: "account_developer_authorizations",
                newName: "IX_account_developer_authorizations_AuthorizedDevelopersId");

            migrationBuilder.AddForeignKey(
                name: "FK_account_developer_authorizations_accounts_AuthorizedAccount~",
                table: "account_developer_authorizations",
                column: "AuthorizedAccountsId",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_developer_authorizations_developers_AuthorizedDevel~",
                table: "account_developer_authorizations",
                column: "AuthorizedDevelopersId",
                principalTable: "developers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_organization_roles_accounts_MembersId",
                table: "account_organization_roles",
                column: "MembersId",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_account_organization_roles_organization_roles_RolesId",
                table: "account_organization_roles",
                column: "RolesId",
                principalTable: "organization_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_application_accounts_ApplicationAccountId",
                table: "sessions",
                column: "ApplicationAccountId",
                principalTable: "application_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_emails_users_UserId",
                table: "user_emails",
                column: "UserId",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
