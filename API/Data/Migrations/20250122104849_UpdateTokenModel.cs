using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tokens_accounts_account_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_applications_application_id",
                table: "tokens");

            migrationBuilder.AddColumn<bool>(
                name: "is_refreshed",
                table: "tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_revoked",
                table: "tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "parent_token_id",
                table: "tokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_is_revoked",
                table: "tokens",
                column: "is_revoked");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_parent_token_id",
                table: "tokens",
                column: "parent_token_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_accounts_account_id",
                table: "tokens",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_applications_application_id",
                table: "tokens",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_tokens_parent_token_id",
                table: "tokens",
                column: "parent_token_id",
                principalTable: "tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tokens_accounts_account_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_applications_application_id",
                table: "tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tokens_tokens_parent_token_id",
                table: "tokens");

            migrationBuilder.DropIndex(
                name: "IX_tokens_is_revoked",
                table: "tokens");

            migrationBuilder.DropIndex(
                name: "IX_tokens_parent_token_id",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "is_refreshed",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "is_revoked",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "parent_token_id",
                table: "tokens");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_accounts_account_id",
                table: "tokens",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_applications_application_id",
                table: "tokens",
                column: "application_id",
                principalTable: "applications",
                principalColumn: "id");
        }
    }
}
