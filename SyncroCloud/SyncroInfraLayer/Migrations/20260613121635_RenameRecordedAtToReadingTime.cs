using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class RenameRecordedAtToReadingTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecordedAt",
                table: "DeviceReadings",
                newName: "ReadingTime");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceReadings_RecordedAt",
                table: "DeviceReadings",
                newName: "IX_DeviceReadings_ReadingTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReadingTime",
                table: "DeviceReadings",
                newName: "RecordedAt");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceReadings_ReadingTime",
                table: "DeviceReadings",
                newName: "IX_DeviceReadings_RecordedAt");
        }
    }
}
