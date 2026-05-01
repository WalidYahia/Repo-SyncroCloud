using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnitTypeAddSensorPaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_UnitType_Type",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "DeviceSensors");

            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "DeviceSensors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataPath",
                table: "DeviceSensors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InchingPath",
                table: "DeviceSensors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InfoPath",
                table: "DeviceSensors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortNo",
                table: "DeviceSensors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_Type",
                table: "Sensors",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_Type",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "DataPath",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "InchingPath",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "InfoPath",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "PortNo",
                table: "DeviceSensors");

            migrationBuilder.AddColumn<string>(
                name: "UnitType",
                table: "Sensors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitType",
                table: "DeviceSensors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_UnitType_Type",
                table: "Sensors",
                columns: new[] { "UnitType", "Type" },
                unique: true);
        }
    }
}
