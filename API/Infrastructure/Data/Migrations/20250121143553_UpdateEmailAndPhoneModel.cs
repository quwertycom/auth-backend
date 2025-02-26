using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailAndPhoneModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_phone_number",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_user_emails_user_id_is_primary",
                table: "user_emails");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_primary",
                table: "user_emails");

            migrationBuilder.AlterColumn<long>(
                name: "email_id",
                table: "verification_sessions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "phone_id",
                table: "verification_sessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "user_emails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "user_phone_numbers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    phone = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_phone_numbers", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_phone_numbers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_verification_sessions_phone_id",
                table: "verification_sessions",
                column: "phone_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "type = 0");

            migrationBuilder.CreateIndex(
                name: "IX_user_phone_numbers_phone",
                table: "user_phone_numbers",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_phone_numbers_user_id",
                table: "user_phone_numbers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_phone_numbers_user_id_type",
                table: "user_phone_numbers",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "type = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_verification_sessions_user_phone_numbers_phone_id",
                table: "verification_sessions",
                column: "phone_id",
                principalTable: "user_phone_numbers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_verification_sessions_user_phone_numbers_phone_id",
                table: "verification_sessions");

            migrationBuilder.DropTable(
                name: "user_phone_numbers");

            migrationBuilder.DropIndex(
                name: "IX_verification_sessions_phone_id",
                table: "verification_sessions");

            migrationBuilder.DropIndex(
                name: "IX_user_emails_user_id_type",
                table: "user_emails");

            migrationBuilder.DropColumn(
                name: "phone_id",
                table: "verification_sessions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "user_emails");

            migrationBuilder.AlterColumn<long>(
                name: "email_id",
                table: "verification_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_primary",
                table: "user_emails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number",
                table: "users",
                column: "phone_number",
                unique: true,
                filter: "\"PhoneNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_emails_user_id_is_primary",
                table: "user_emails",
                columns: new[] { "user_id", "is_primary" },
                unique: true,
                filter: "is_primary = true");
        }
    }
}
