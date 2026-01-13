// <copyright file="20260113103434_AddCloudStorageSupport.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileConversionApi.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCloudStorageSupport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "StorageLocation",
            table: "ConversionJobs",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "CloudStorageKey",
            table: "ConversionJobs",
            type: "character varying(1024)",
            maxLength: 1024,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ConversionJobs_StorageLocation",
            table: "ConversionJobs",
            column: "StorageLocation");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ConversionJobs_StorageLocation",
            table: "ConversionJobs");

        migrationBuilder.DropColumn(
            name: "CloudStorageKey",
            table: "ConversionJobs");

        migrationBuilder.DropColumn(
            name: "StorageLocation",
            table: "ConversionJobs");
    }
}
