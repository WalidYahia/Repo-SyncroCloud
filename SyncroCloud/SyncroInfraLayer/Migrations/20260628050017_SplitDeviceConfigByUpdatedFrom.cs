using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class SplitDeviceConfigByUpdatedFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceConfigs_DeviceId_ConfigType",
                table: "DeviceConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfigs_DeviceId_ConfigType_UpdatedFrom",
                table: "DeviceConfigs",
                columns: new[] { "DeviceId", "ConfigType", "UpdatedFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceConfigs_DeviceId_ConfigType_UpdatedFrom",
                table: "DeviceConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfigs_DeviceId_ConfigType",
                table: "DeviceConfigs",
                columns: new[] { "DeviceId", "ConfigType" },
                unique: true);
        }
    }
}
