using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResetPasswordRequestConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResetPasswordRequests_user_emails_EmailAddressId",
                table: "ResetPasswordRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ResetPasswordRequests_users_user_id",
                table: "ResetPasswordRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResetPasswordRequests",
                table: "ResetPasswordRequests");

            migrationBuilder.DropIndex(
                name: "IX_ResetPasswordRequests_EmailAddressId",
                table: "ResetPasswordRequests");

            migrationBuilder.DropColumn(
                name: "EmailAddressId",
                table: "ResetPasswordRequests");

            migrationBuilder.RenameTable(
                name: "ResetPasswordRequests",
                newName: "reset_password_requests");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPasswordRequests_user_id",
                table: "reset_password_requests",
                newName: "IX_reset_password_requests_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reset_password_requests",
                table: "reset_password_requests",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_reset_password_requests_email_id",
                table: "reset_password_requests",
                column: "email_id");

            migrationBuilder.CreateIndex(
                name: "IX_reset_password_requests_otp",
                table: "reset_password_requests",
                column: "otp");

            migrationBuilder.AddForeignKey(
                name: "FK_reset_password_requests_user_emails_email_id",
                table: "reset_password_requests",
                column: "email_id",
                principalTable: "user_emails",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reset_password_requests_users_user_id",
                table: "reset_password_requests",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reset_password_requests_user_emails_email_id",
                table: "reset_password_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_reset_password_requests_users_user_id",
                table: "reset_password_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reset_password_requests",
                table: "reset_password_requests");

            migrationBuilder.DropIndex(
                name: "IX_reset_password_requests_email_id",
                table: "reset_password_requests");

            migrationBuilder.DropIndex(
                name: "IX_reset_password_requests_otp",
                table: "reset_password_requests");

            migrationBuilder.RenameTable(
                name: "reset_password_requests",
                newName: "ResetPasswordRequests");

            migrationBuilder.RenameIndex(
                name: "IX_reset_password_requests_user_id",
                table: "ResetPasswordRequests",
                newName: "IX_ResetPasswordRequests_user_id");

            migrationBuilder.AddColumn<long>(
                name: "EmailAddressId",
                table: "ResetPasswordRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResetPasswordRequests",
                table: "ResetPasswordRequests",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_EmailAddressId",
                table: "ResetPasswordRequests",
                column: "EmailAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPasswordRequests_user_emails_EmailAddressId",
                table: "ResetPasswordRequests",
                column: "EmailAddressId",
                principalTable: "user_emails",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPasswordRequests_users_user_id",
                table: "ResetPasswordRequests",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
