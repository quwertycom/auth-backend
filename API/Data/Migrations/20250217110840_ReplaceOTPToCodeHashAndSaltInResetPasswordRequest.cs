using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOTPToCodeHashAndSaltInResetPasswordRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reset_password_requests_otp",
                table: "reset_password_requests");

            migrationBuilder.RenameColumn(
                name: "otp",
                table: "reset_password_requests",
                newName: "salt");

            migrationBuilder.AddColumn<string>(
                name: "code_hash",
                table: "reset_password_requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_reset_password_requests_code_hash",
                table: "reset_password_requests",
                column: "code_hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reset_password_requests_code_hash",
                table: "reset_password_requests");

            migrationBuilder.DropColumn(
                name: "code_hash",
                table: "reset_password_requests");

            migrationBuilder.RenameColumn(
                name: "salt",
                table: "reset_password_requests",
                newName: "otp");

            migrationBuilder.CreateIndex(
                name: "IX_reset_password_requests_otp",
                table: "reset_password_requests",
                column: "otp");
        }
    }
}
