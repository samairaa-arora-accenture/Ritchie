using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Richie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVaultRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RecoverySalt",
                table: "VaultKeys",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecoveryWrappedDek",
                table: "VaultKeys",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecoverySalt",
                table: "VaultKeys");

            migrationBuilder.DropColumn(
                name: "RecoveryWrappedDek",
                table: "VaultKeys");
        }
    }
}
