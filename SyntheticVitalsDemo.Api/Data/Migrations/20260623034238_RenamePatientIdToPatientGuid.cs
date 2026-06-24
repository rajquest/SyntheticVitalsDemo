using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticVitalsDemo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePatientIdToPatientGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Patients",
                newName: "PatientGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PatientGuid",
                table: "Patients",
                newName: "Id");
        }
    }
}
