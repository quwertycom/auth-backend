using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedNamesOfVerificationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEmails_Users_UserId",
                table: "UserEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhoneNumbers_Users_UserId",
                table: "UserPhoneNumbers");

            migrationBuilder.DropTable(
                name: "ResetPasswordRequests");

            migrationBuilder.DropTable(
                name: "VerifyEmailSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPhoneNumbers",
                table: "UserPhoneNumbers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserEmails",
                table: "UserEmails");

            migrationBuilder.RenameTable(
                name: "UserPhoneNumbers",
                newName: "PhoneNumbers");

            migrationBuilder.RenameTable(
                name: "UserEmails",
                newName: "EmailAddresses");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhoneNumbers_Value",
                table: "PhoneNumbers",
                newName: "IX_PhoneNumbers_Value");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhoneNumbers_UserId",
                table: "PhoneNumbers",
                newName: "IX_PhoneNumbers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserEmails_Value",
                table: "EmailAddresses",
                newName: "IX_EmailAddresses_Value");

            migrationBuilder.RenameIndex(
                name: "IX_UserEmails_UserId",
                table: "EmailAddresses",
                newName: "IX_EmailAddresses_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhoneNumbers",
                table: "PhoneNumbers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailAddresses",
                table: "EmailAddresses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmailVerificationRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    EmailId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerificationRequests_EmailAddresses_EmailId",
                        column: x => x.EmailId,
                        principalTable: "EmailAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailVerificationRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    EmailId = table.Column<long>(type: "bigint", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequests_EmailAddresses_EmailId",
                        column: x => x.EmailId,
                        principalTable: "EmailAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationRequests_Code",
                table: "EmailVerificationRequests",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationRequests_EmailId",
                table: "EmailVerificationRequests",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationRequests_ExpiresAt",
                table: "EmailVerificationRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationRequests_UserId",
                table: "EmailVerificationRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_CodeHash",
                table: "PasswordResetRequests",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_EmailId",
                table: "PasswordResetRequests",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_ExpiresAt",
                table: "PasswordResetRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequests_UserId",
                table: "PasswordResetRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailAddresses_Users_UserId",
                table: "EmailAddresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneNumbers_Users_UserId",
                table: "PhoneNumbers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailAddresses_Users_UserId",
                table: "EmailAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_PhoneNumbers_Users_UserId",
                table: "PhoneNumbers");

            migrationBuilder.DropTable(
                name: "EmailVerificationRequests");

            migrationBuilder.DropTable(
                name: "PasswordResetRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhoneNumbers",
                table: "PhoneNumbers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailAddresses",
                table: "EmailAddresses");

            migrationBuilder.RenameTable(
                name: "PhoneNumbers",
                newName: "UserPhoneNumbers");

            migrationBuilder.RenameTable(
                name: "EmailAddresses",
                newName: "UserEmails");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneNumbers_Value",
                table: "UserPhoneNumbers",
                newName: "IX_UserPhoneNumbers_Value");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneNumbers_UserId",
                table: "UserPhoneNumbers",
                newName: "IX_UserPhoneNumbers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailAddresses_Value",
                table: "UserEmails",
                newName: "IX_UserEmails_Value");

            migrationBuilder.RenameIndex(
                name: "IX_EmailAddresses_UserId",
                table: "UserEmails",
                newName: "IX_UserEmails_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPhoneNumbers",
                table: "UserPhoneNumbers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserEmails",
                table: "UserEmails",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ResetPasswordRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmailId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswordRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResetPasswordRequests_UserEmails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "UserEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResetPasswordRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerifyEmailSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmailId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifyEmailSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerifyEmailSessions_UserEmails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "UserEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VerifyEmailSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_CodeHash",
                table: "ResetPasswordRequests",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_EmailId",
                table: "ResetPasswordRequests",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_ExpiresAt",
                table: "ResetPasswordRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordRequests_UserId",
                table: "ResetPasswordRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VerifyEmailSessions_Code",
                table: "VerifyEmailSessions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_VerifyEmailSessions_EmailId",
                table: "VerifyEmailSessions",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_VerifyEmailSessions_ExpiresAt",
                table: "VerifyEmailSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_VerifyEmailSessions_UserId",
                table: "VerifyEmailSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEmails_Users_UserId",
                table: "UserEmails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhoneNumbers_Users_UserId",
                table: "UserPhoneNumbers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
