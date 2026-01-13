using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileConversionApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRateLimitSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRateLimitSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StandardPolicyPermitLimit = table.Column<int>(type: "integer", nullable: true),
                    StandardPolicyWindowMinutes = table.Column<int>(type: "integer", nullable: true),
                    ConversionPolicyPermitLimit = table.Column<int>(type: "integer", nullable: true),
                    ConversionPolicyWindowMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRateLimitSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRateLimitSettings_UserId",
                table: "UserRateLimitSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRateLimitSettings");
        }
    }
}
