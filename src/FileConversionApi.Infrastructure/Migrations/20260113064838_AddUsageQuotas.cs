using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileConversionApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsageQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsageQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    ConversionsUsed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConversionsLimit = table.Column<int>(type: "integer", nullable: false),
                    BytesProcessed = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    BytesLimit = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageQuotas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsageQuotas_UserId",
                table: "UsageQuotas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageQuotas_UserId_Year_Month",
                table: "UsageQuotas",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsageQuotas");
        }
    }
}
