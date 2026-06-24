using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticVitalsDemo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device",
                columns: table => new
                {
                    device_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_id = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    imei_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bluetooth_address = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_time_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_time_last_updated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_time_deactivated = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_time_patient_assigned = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    patient_guid = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device", x => new { x.device_type, x.device_id });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_imei",
                table: "device",
                column: "imei_number");

            migrationBuilder.CreateIndex(
                name: "idx_patient",
                table: "device",
                column: "patient_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device");
        }
    }
}
