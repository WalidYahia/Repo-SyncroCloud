using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class DeviceSensorIdentityPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceId_SensorId",
                table: "DeviceReadings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceSensors",
                table: "DeviceSensors");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "DeviceSensors",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "DeviceSensorId",
                table: "DeviceReadings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceSensors",
                table: "DeviceSensors",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSensors_Unique",
                table: "DeviceSensors",
                columns: new[] { "DeviceId", "SensorId", "SwitchNo", "UnitId", "Address", "Port" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId",
                principalTable: "DeviceSensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceSensors",
                table: "DeviceSensors");

            migrationBuilder.DropIndex(
                name: "IX_DeviceSensors_Unique",
                table: "DeviceSensors");

            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceSensors",
                table: "DeviceSensors",
                columns: new[] { "DeviceId", "SensorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceId_SensorId",
                table: "DeviceReadings",
                columns: new[] { "DeviceId", "SensorId" },
                principalTable: "DeviceSensors",
                principalColumns: new[] { "DeviceId", "SensorId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
