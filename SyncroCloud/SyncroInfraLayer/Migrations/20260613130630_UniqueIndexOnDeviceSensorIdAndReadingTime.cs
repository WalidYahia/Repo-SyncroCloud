using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexOnDeviceSensorIdAndReadingTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove duplicate rows, keeping the one with the smallest ctid per (DeviceSensorId, ReadingTime)
            migrationBuilder.Sql("""
                DELETE FROM "DeviceReadings"
                WHERE ctid NOT IN (
                    SELECT MIN(ctid)
                    FROM "DeviceReadings"
                    GROUP BY "DeviceSensorId", "ReadingTime"
                );
                """);

            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_ReadingTime",
                table: "DeviceReadings");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceSensorId_ReadingTime",
                table: "DeviceReadings",
                columns: new[] { "DeviceSensorId", "ReadingTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_DeviceSensorId_ReadingTime",
                table: "DeviceReadings");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_ReadingTime",
                table: "DeviceReadings",
                column: "ReadingTime");
        }
    }
}
