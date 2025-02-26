using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelColumnNamesToUnderscoreType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_organizations_OrganizationId",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_accounts_users_UserId",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_application_accounts_accounts_AccountId",
                table: "application_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_application_accounts_applications_ApplicationId",
                table: "application_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_applications_developers_DeveloperId",
                table: "applications");

            migrationBuilder.DropForeignKey(
                name: "FK_developers_organizations_OrganizationId",
                table: "developers");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_accounts_AccountId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_applications_ApplicationId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_UserId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_organization_roles_organizations_OrganizationId",
                table: "organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_accounts_AccountId",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_applications_ApplicationId",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_users_UserId",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_accounts_AccountId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_application_accounts_ApplicationAccountId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_applications_ApplicationId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_sessions_SessionId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_users_UserId",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_verification_sessions_user_emails_EmailId",
                table: "verification_sessions");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "verification_sessions",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "verification_sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ExpiryMinutes",
                table: "verification_sessions",
                newName: "expiry_minutes");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "verification_sessions",
                newName: "email_id");

            migrationBuilder.RenameIndex(
                name: "IX_verification_sessions_Code",
                table: "verification_sessions",
                newName: "IX_verification_sessions_code");

            migrationBuilder.RenameIndex(
                name: "IX_verification_sessions_EmailId",
                table: "verification_sessions",
                newName: "IX_verification_sessions_email_id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_emails",
                newName: "user_email_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TokenString",
                table: "tokens",
                newName: "token_string");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "tokens",
                newName: "session_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "tokens",
                newName: "application_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationAccountId",
                table: "tokens",
                newName: "application_account_id");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "tokens",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_target_UserId",
                table: "tokens",
                newName: "IX_tokens_target_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_UserId",
                table: "tokens",
                newName: "IX_tokens_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_TokenString",
                table: "tokens",
                newName: "IX_tokens_token_string");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_SessionId",
                table: "tokens",
                newName: "IX_tokens_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_ApplicationId",
                table: "tokens",
                newName: "IX_tokens_application_id");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_ApplicationAccountId",
                table: "tokens",
                newName: "IX_tokens_application_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_AccountId",
                table: "tokens",
                newName: "IX_tokens_account_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "sessions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "sessions",
                newName: "application_id");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "sessions",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_target_UserId",
                table: "sessions",
                newName: "IX_sessions_target_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_UserId",
                table: "sessions",
                newName: "IX_sessions_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_ApplicationId",
                table: "sessions",
                newName: "IX_sessions_application_id");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_AccountId_ApplicationId",
                table: "sessions",
                newName: "IX_sessions_account_id_application_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "organizations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "organizations",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "organizations",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_organizations_Name",
                table: "organizations",
                newName: "IX_organizations_name");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "organization_roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "organization_roles",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "organization_roles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "organization_roles",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "organization_roles",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_organization_roles_OrganizationId",
                table: "organization_roles",
                newName: "IX_organization_roles_organization_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "notifications",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "notifications",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notifications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "notifications",
                newName: "is_read");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "notifications",
                newName: "application_id");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "notifications",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_UserId_IsRead",
                table: "notifications",
                newName: "IX_notifications_user_id_is_read");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                newName: "IX_notifications_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_ApplicationId",
                table: "notifications",
                newName: "IX_notifications_application_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_AccountId_ApplicationId",
                table: "notifications",
                newName: "IX_notifications_account_id_application_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "developers",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "developers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "developers",
                newName: "organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_developers_Name",
                table: "developers",
                newName: "IX_developers_name");

            migrationBuilder.RenameIndex(
                name: "IX_developers_type_OrganizationId",
                table: "developers",
                newName: "IX_developers_type_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_developers_OrganizationId",
                table: "developers",
                newName: "IX_developers_organization_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "applications",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "applications",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "applications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RedirectUri",
                table: "applications",
                newName: "redirect_uri");

            migrationBuilder.RenameColumn(
                name: "IconUrl",
                table: "applications",
                newName: "icon_url");

            migrationBuilder.RenameColumn(
                name: "DeveloperId",
                table: "applications",
                newName: "developer_id");

            migrationBuilder.RenameIndex(
                name: "IX_applications_Name",
                table: "applications",
                newName: "IX_applications_name");

            migrationBuilder.RenameIndex(
                name: "IX_applications_DeveloperId_status",
                table: "applications",
                newName: "IX_applications_developer_id_status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "application_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "application_accounts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "application_accounts",
                newName: "application_id");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "application_accounts",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_application_accounts_ApplicationId",
                table: "application_accounts",
                newName: "IX_application_accounts_application_id");

            migrationBuilder.RenameIndex(
                name: "IX_application_accounts_AccountId",
                table: "application_accounts",
                newName: "IX_application_accounts_account_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "accounts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "accounts",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "AccountName",
                table: "accounts",
                newName: "account_name");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_OrganizationId",
                table: "accounts",
                newName: "IX_accounts_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_AccountName",
                table: "accounts",
                newName: "IX_accounts_account_name");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_organizations_organization_id",
                table: "accounts",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_users_user_id",
                table: "accounts",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_application_accounts_accounts_account_id",
                table: "application_accounts",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_application_accounts_applications_application_id",
                table: "application_accounts",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_applications_developers_developer_id",
                table: "applications",
                column: "developer_id",
                principalTable: "developers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_developers_organizations_organization_id",
                table: "developers",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

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
                name: "FK_notifications_users_user_id",
                table: "notifications",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_organization_roles_organizations_organization_id",
                table: "organization_roles",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_users_user_id",
                table: "sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_accounts_account_id",
                table: "tokens",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_application_accounts_application_account_id",
                table: "tokens",
                column: "application_account_id",
                principalTable: "application_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_applications_application_id",
                table: "tokens",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_sessions_session_id",
                table: "tokens",
                column: "session_id",
                principalTable: "sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_users_user_id",
                table: "tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_verification_sessions_user_emails_email_id",
                table: "verification_sessions",
                column: "email_id",
                principalTable: "user_emails",
                principalColumn: "user_email_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_organizations_organization_id",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_accounts_users_user_id",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_application_accounts_accounts_account_id",
                table: "application_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_application_accounts_applications_application_id",
                table: "application_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_applications_developers_developer_id",
                table: "applications");

            migrationBuilder.DropForeignKey(
                name: "FK_developers_organizations_organization_id",
                table: "developers");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_accounts_account_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_applications_application_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_user_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_organization_roles_organizations_organization_id",
                table: "organization_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_accounts_account_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_applications_application_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_users_user_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_accounts_account_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_application_accounts_application_account_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_applications_application_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_sessions_session_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_users_user_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_verification_sessions_user_emails_email_id",
                table: "verification_sessions");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "verification_sessions",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "verification_sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "expiry_minutes",
                table: "verification_sessions",
                newName: "ExpiryMinutes");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "verification_sessions",
                newName: "EmailId");

            migrationBuilder.RenameIndex(
                name: "IX_verification_sessions_code",
                table: "verification_sessions",
                newName: "IX_verification_sessions_Code");

            migrationBuilder.RenameIndex(
                name: "IX_verification_sessions_email_id",
                table: "verification_sessions",
                newName: "IX_verification_sessions_EmailId");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_email_id",
                table: "user_emails",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "tokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "token_string",
                table: "tokens",
                newName: "TokenString");

            migrationBuilder.RenameColumn(
                name: "session_id",
                table: "tokens",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "tokens",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "application_account_id",
                table: "tokens",
                newName: "ApplicationAccountId");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "tokens",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_user_id",
                table: "tokens",
                newName: "IX_tokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_token_string",
                table: "tokens",
                newName: "IX_tokens_TokenString");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_target_user_id",
                table: "tokens",
                newName: "IX_tokens_target_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_session_id",
                table: "tokens",
                newName: "IX_tokens_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_application_id",
                table: "tokens",
                newName: "IX_tokens_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_application_account_id",
                table: "tokens",
                newName: "IX_tokens_ApplicationAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_account_id",
                table: "tokens",
                newName: "IX_tokens_AccountId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "sessions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "sessions",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "sessions",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                newName: "IX_sessions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_target_user_id",
                table: "sessions",
                newName: "IX_sessions_target_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_application_id",
                table: "sessions",
                newName: "IX_sessions_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_account_id_application_id",
                table: "sessions",
                newName: "IX_sessions_AccountId_ApplicationId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "organizations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "organizations",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "organizations",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_organizations_name",
                table: "organizations",
                newName: "IX_organizations_Name");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "organization_roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "organization_roles",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "organization_roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "organization_roles",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "organization_roles",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_organization_roles_organization_id",
                table: "organization_roles",
                newName: "IX_organization_roles_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "is_read",
                table: "notifications",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "notifications",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "notifications",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_user_id_is_read",
                table: "notifications",
                newName: "IX_notifications_UserId_IsRead");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                newName: "IX_notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_application_id",
                table: "notifications",
                newName: "IX_notifications_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_account_id_application_id",
                table: "notifications",
                newName: "IX_notifications_AccountId_ApplicationId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "developers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "developers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "developers",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_developers_name",
                table: "developers",
                newName: "IX_developers_Name");

            migrationBuilder.RenameIndex(
                name: "IX_developers_type_organization_id",
                table: "developers",
                newName: "IX_developers_type_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_developers_organization_id",
                table: "developers",
                newName: "IX_developers_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "applications",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "applications",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "applications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "redirect_uri",
                table: "applications",
                newName: "RedirectUri");

            migrationBuilder.RenameColumn(
                name: "icon_url",
                table: "applications",
                newName: "IconUrl");

            migrationBuilder.RenameColumn(
                name: "developer_id",
                table: "applications",
                newName: "DeveloperId");

            migrationBuilder.RenameIndex(
                name: "IX_applications_name",
                table: "applications",
                newName: "IX_applications_Name");

            migrationBuilder.RenameIndex(
                name: "IX_applications_developer_id_status",
                table: "applications",
                newName: "IX_applications_DeveloperId_status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "application_accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "application_accounts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "application_accounts",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "application_accounts",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_application_accounts_application_id",
                table: "application_accounts",
                newName: "IX_application_accounts_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_application_accounts_account_id",
                table: "application_accounts",
                newName: "IX_application_accounts_AccountId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "accounts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "accounts",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "account_name",
                table: "accounts",
                newName: "AccountName");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_organization_id",
                table: "accounts",
                newName: "IX_accounts_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_account_name",
                table: "accounts",
                newName: "IX_accounts_AccountName");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_organizations_OrganizationId",
                table: "accounts",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_users_UserId",
                table: "accounts",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_application_accounts_accounts_AccountId",
                table: "application_accounts",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_application_accounts_applications_ApplicationId",
                table: "application_accounts",
                column: "ApplicationId",
                principalTable: "applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_applications_developers_DeveloperId",
                table: "applications",
                column: "DeveloperId",
                principalTable: "developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_developers_organizations_OrganizationId",
                table: "developers",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_accounts_AccountId",
                table: "notifications",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_applications_ApplicationId",
                table: "notifications",
                column: "ApplicationId",
                principalTable: "applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_UserId",
                table: "notifications",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_organization_roles_organizations_OrganizationId",
                table: "organization_roles",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_accounts_AccountId",
                table: "sessions",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_applications_ApplicationId",
                table: "sessions",
                column: "ApplicationId",
                principalTable: "applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_users_UserId",
                table: "sessions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_accounts_AccountId",
                table: "tokens",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_application_accounts_ApplicationAccountId",
                table: "tokens",
                column: "ApplicationAccountId",
                principalTable: "application_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_users_UserId",
                table: "tokens",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_verification_sessions_user_emails_EmailId",
                table: "verification_sessions",
                column: "EmailId",
                principalTable: "user_emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
