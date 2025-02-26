using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnumHandlingToUseStrings : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "user_phone_numbers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "user_emails",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_phone_numbers_user_id_type",
                table: "user_phone_numbers");

            migrationBuilder.DropIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "user_phone_numbers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "user_emails",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

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
    }
}
