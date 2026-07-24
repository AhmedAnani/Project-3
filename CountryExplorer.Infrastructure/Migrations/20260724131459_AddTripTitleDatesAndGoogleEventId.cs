using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripTitleDatesAndGoogleEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TripBucketItems]') AND name = N'TargetDate')
                BEGIN
                    DECLARE @var sysname;
                    SELECT @var = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TripBucketItems]') AND [c].[name] = N'TargetDate');
                    IF @var IS NOT NULL EXEC(N'ALTER TABLE [TripBucketItems] DROP CONSTRAINT [' + @var + '];');
                    ALTER TABLE [TripBucketItems] DROP COLUMN [TargetDate];
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TripBucketItems]') AND name = N'EndDate')
                BEGIN
                    ALTER TABLE [TripBucketItems] ADD [EndDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.000';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TripBucketItems]') AND name = N'GoogleEventId')
                BEGIN
                    ALTER TABLE [TripBucketItems] ADD [GoogleEventId] nvarchar(max) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TripBucketItems]') AND name = N'StartDate')
                BEGIN
                    ALTER TABLE [TripBucketItems] ADD [StartDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.000';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TripBucketItems]') AND name = N'Title')
                BEGIN
                    ALTER TABLE [TripBucketItems] ADD [Title] nvarchar(max) NOT NULL DEFAULT N'';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "TripBucketItems");

            migrationBuilder.DropColumn(
                name: "GoogleEventId",
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
