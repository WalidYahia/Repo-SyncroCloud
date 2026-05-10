using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceActionLogAndIsPendingSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPendingSync",
                table: "DeviceSensors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DeviceActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstalledSensorId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceActionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceActionLogs_DeviceId",
                table: "DeviceActionLogs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceActionLogs_InstalledSensorId",
                table: "DeviceActionLogs",
                column: "InstalledSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceActionLogs_Timestamp",
                table: "DeviceActionLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceActionLogs");

            migrationBuilder.DropColumn(
                name: "IsPendingSync",
                table: "DeviceSensors");
        }
    }
}
