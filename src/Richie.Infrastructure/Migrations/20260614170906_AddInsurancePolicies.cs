using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Richie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInsurancePolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsurancePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PolicyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PolicyNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    CoverageAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AnnualPremium = table.Column<decimal>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Nominee = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RenewalNotifiedForDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsurancePolicies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_UserId_RenewalDate",
                table: "InsurancePolicies",
                columns: new[] { "UserId", "RenewalDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsurancePolicies");
        }
    }
}
