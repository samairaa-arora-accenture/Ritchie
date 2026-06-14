using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Richie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SessionLockMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IncludeJewelleryInPortfolio = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackupFrequency = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DisabledNotificationTypes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    LastBackupUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
