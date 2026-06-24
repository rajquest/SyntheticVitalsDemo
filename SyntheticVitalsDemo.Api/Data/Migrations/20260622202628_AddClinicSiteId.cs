using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticVitalsDemo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicSiteId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SiteId",
                table: "Clinics",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Clinics");
        }
    }
}
