using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEnumTypeHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_phone_numbers_user_id_type",
                table: "user_phone_numbers");

            migrationBuilder.DropIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails");

            migrationBuilder.CreateIndex(
                name: "IX_user_phone_numbers_user_id_type",
                table: "user_phone_numbers",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "type = 0");

            migrationBuilder.CreateIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "type = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_phone_numbers_user_id_type",
                table: "user_phone_numbers");

            migrationBuilder.DropIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails");

            migrationBuilder.CreateIndex(
                name: "IX_user_phone_numbers_user_id_type",
                table: "user_phone_numbers",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "type = 'Primary'");

            migrationBuilder.CreateIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "type = 'Primary'");
        }
    }
}
