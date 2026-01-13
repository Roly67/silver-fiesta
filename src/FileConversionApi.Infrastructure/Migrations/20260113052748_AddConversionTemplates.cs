using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileConversionApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConversionTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConversionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TargetFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OptionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversionTemplates_UserId",
                table: "ConversionTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversionTemplates_UserId_Name",
                table: "ConversionTemplates",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversionTemplates_UserId_TargetFormat",
                table: "ConversionTemplates",
                columns: new[] { "UserId", "TargetFormat" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionTemplates");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");
        }
    }
}
