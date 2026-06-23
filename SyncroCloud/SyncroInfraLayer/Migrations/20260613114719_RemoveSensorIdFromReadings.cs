using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSensorIdFromReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_DeviceId_SensorId",
                table: "DeviceReadings");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "DeviceReadings");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceReadings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceReadings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "SensorId",
                table: "DeviceReadings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceId_SensorId",
                table: "DeviceReadings",
                columns: new[] { "DeviceId", "SensorId" });
        }
    }
}
