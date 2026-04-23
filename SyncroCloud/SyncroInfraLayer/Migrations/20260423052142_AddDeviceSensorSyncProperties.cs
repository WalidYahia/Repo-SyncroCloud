using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceSensorSyncProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EventChangeDelta",
                table: "DeviceSensors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EventChangeSync",
                table: "DeviceSensors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SyncPeriodicity",
                table: "DeviceSensors",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventChangeDelta",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "EventChangeSync",
                table: "DeviceSensors");

            migrationBuilder.DropColumn(
                name: "SyncPeriodicity",
                table: "DeviceSensors");
        }
    }
}
