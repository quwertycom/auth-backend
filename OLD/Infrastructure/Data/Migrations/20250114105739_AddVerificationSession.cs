using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "verification_sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmailId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiryMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_verification_sessions_user_emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "user_emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_verification_sessions_Code",
                table: "verification_sessions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_verification_sessions_EmailId",
                table: "verification_sessions",
                column: "EmailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "verification_sessions");
        }
    }
}
