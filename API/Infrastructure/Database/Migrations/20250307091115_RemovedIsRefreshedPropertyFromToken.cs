using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovedIsRefreshedPropertyFromToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRefreshed",
                table: "Tokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRefreshed",
                table: "Tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
