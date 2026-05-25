using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticVitalsDemo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPulmonaryPressureTrendScenario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrendScenario",
                table: "VitalsSubmissions",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "NormalStable")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrendScenario",
                table: "VitalsSubmissions");
        }
    }
}
