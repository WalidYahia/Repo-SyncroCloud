using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class SensorConfigRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.DropTable(
                name: "DeviceSensors");

            migrationBuilder.DropIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.CreateTable(
                name: "DeviceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConfigType = table.Column<int>(type: "integer", nullable: false),
                    ConfigVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Config = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    UpdatedFrom = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceConfigs_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfigs_DeviceId_ConfigType",
                table: "DeviceConfigs",
                columns: new[] { "DeviceId", "ConfigType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceConfigs");

            migrationBuilder.CreateTable(
                name: "DeviceSensors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", nullable: false),
                    InstalledById = table.Column<Guid>(type: "uuid", nullable: true),
                    SensorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<int>(type: "integer", nullable: true),
                    BaseUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventChangeDelta = table.Column<double>(type: "double precision", nullable: true),
                    EventChangeSync = table.Column<bool>(type: "boolean", nullable: false),
                    InchingModeWidthInMs = table.Column<int>(type: "integer", nullable: false),
                    InchingPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InfoPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsInInchingMode = table.Column<bool>(type: "boolean", nullable: false),
                    IsPendingSync = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastReading = table.Column<string>(type: "jsonb", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    PortNo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Protocol = table.Column<int>(type: "integer", nullable: false),
                    SensorType = table.Column<string>(type: "text", nullable: false),
                    SwitchNo = table.Column<string>(type: "text", nullable: false),
                    SyncPeriodicity = table.Column<int>(type: "integer", nullable: true),
                    UnitId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSensors_AspNetUsers_InstalledById",
                        column: x => x.InstalledById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeviceSensors_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceSensors_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "SensorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSensors_InstalledById",
                table: "DeviceSensors",
                column: "InstalledById");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSensors_SensorId",
                table: "DeviceSensors",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSensors_Unique",
                table: "DeviceSensors",
                columns: new[] { "DeviceId", "SensorId", "SwitchNo", "UnitId", "Address", "Port" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId",
                principalTable: "DeviceSensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
