using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InternetQualityScore = table.Column<int>(type: "int", nullable: false),
                    SafetyIndex = table.Column<int>(type: "int", nullable: false),
                    CostOfLivingIndex = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_CountryCode_CityName",
                table: "Destinations",
                columns: new[] { "CountryCode", "CityName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Destinations");
        }
    }
}
