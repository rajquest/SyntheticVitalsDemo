using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticVitalsDemo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePatientContactWithCurrentVitals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "DiastolicBp",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeartRate",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Spo2",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SystolicBp",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightLbs",
                table: "Patients",
                type: "decimal(6,1)",
                precision: 6,
                scale: 1,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiastolicBp",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HeartRate",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Spo2",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "SystolicBp",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "WeightLbs",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Patients",
                type: "varchar(160)",
                maxLength: 160,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Patients",
                type: "varchar(24)",
                maxLength: 24,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
