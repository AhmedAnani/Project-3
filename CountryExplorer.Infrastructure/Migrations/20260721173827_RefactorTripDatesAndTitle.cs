using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTripDatesAndTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetDate",
                table: "TripBucketItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "TripBucketItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "TripBucketItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TripBucketItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "TripBucketItems");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TripBucketItems");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TripBucketItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetDate",
                table: "TripBucketItems",
                type: "datetime2",
                nullable: true);
        }
    }
}
