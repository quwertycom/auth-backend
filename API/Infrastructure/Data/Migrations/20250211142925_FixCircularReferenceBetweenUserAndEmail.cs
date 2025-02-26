using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCircularReferenceBetweenUserAndEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_emails_users_user_id",
                table: "user_emails");

            migrationBuilder.CreateTable(
                name: "ResetPasswordRequests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    email_id = table.Column<long>(type: "bigint", nullable: false),
                    EmailAddressId = table.Column<long>(type: "bigint", nullable: false),
                    otp = table.Column<string>(type: "text", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswordRequests", x => x.id);
                    table.ForeignKey(
                        name: "FK_ResetPasswordRequests_user_emails_EmailAddressId",
                        column: x => x.EmailAddressId,
                        principalTable: "user_emails",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResetPasswordRequests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_EmailAddressId",
                table: "ResetPasswordRequests",
                column: "EmailAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_user_id",
                table: "ResetPasswordRequests",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEmails_Users",
                table: "user_emails",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEmails_Users",
                table: "user_emails");

            migrationBuilder.DropTable(
                name: "ResetPasswordRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_user_emails_users_user_id",
                table: "user_emails",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
