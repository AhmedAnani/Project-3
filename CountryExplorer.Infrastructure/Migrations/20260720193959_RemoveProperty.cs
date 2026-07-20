using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "RefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
