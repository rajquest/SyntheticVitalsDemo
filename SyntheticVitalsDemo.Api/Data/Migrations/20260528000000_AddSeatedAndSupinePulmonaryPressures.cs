using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticVitalsDemo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatedAndSupinePulmonaryPressures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaSystolic",
                table: "VitalsSubmissions",
                newName: "SeatedPaSystolic");

            migrationBuilder.RenameColumn(
                name: "PaDiastolic",
                table: "VitalsSubmissions",
                newName: "SeatedPaDiastolic");

            migrationBuilder.RenameColumn(
                name: "PaMean",
                table: "VitalsSubmissions",
                newName: "SeatedPaMean");

            migrationBuilder.RenameColumn(
                name: "PaSystolic",
                table: "Patients",
                newName: "SeatedPaSystolic");

            migrationBuilder.RenameColumn(
                name: "PaDiastolic",
                table: "Patients",
                newName: "SeatedPaDiastolic");

            migrationBuilder.RenameColumn(
                name: "PaMean",
                table: "Patients",
                newName: "SeatedPaMean");

            migrationBuilder.AddColumn<int>(
                name: "SupinePaDiastolic",
                table: "VitalsSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupinePaMean",
                table: "VitalsSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupinePaSystolic",
                table: "VitalsSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupinePaDiastolic",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupinePaMean",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupinePaSystolic",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupinePaDiastolic",
                table: "VitalsSubmissions");

            migrationBuilder.DropColumn(
                name: "SupinePaMean",
                table: "VitalsSubmissions");

            migrationBuilder.DropColumn(
                name: "SupinePaSystolic",
                table: "VitalsSubmissions");

            migrationBuilder.DropColumn(
                name: "SupinePaDiastolic",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "SupinePaMean",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "SupinePaSystolic",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "SeatedPaSystolic",
                table: "VitalsSubmissions",
                newName: "PaSystolic");

            migrationBuilder.RenameColumn(
                name: "SeatedPaDiastolic",
                table: "VitalsSubmissions",
                newName: "PaDiastolic");

            migrationBuilder.RenameColumn(
                name: "SeatedPaMean",
                table: "VitalsSubmissions",
                newName: "PaMean");

            migrationBuilder.RenameColumn(
                name: "SeatedPaSystolic",
                table: "Patients",
                newName: "PaSystolic");

            migrationBuilder.RenameColumn(
                name: "SeatedPaDiastolic",
                table: "Patients",
                newName: "PaDiastolic");

            migrationBuilder.RenameColumn(
                name: "SeatedPaMean",
                table: "Patients",
                newName: "PaMean");
        }
    }
}
