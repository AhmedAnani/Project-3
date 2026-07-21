using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleEventIdToTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleEventId",
                table: "TripBucketItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleEventId",
                table: "TripBucketItems");
        }
    }
}
