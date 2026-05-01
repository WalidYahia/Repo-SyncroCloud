using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class RenameSwitchSensorType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Sensors\" SET \"Type\" = 'SonOffMiniR3Swich' WHERE \"Type\" = 'Swich'");
            migrationBuilder.Sql("UPDATE \"DeviceSensors\" SET \"SensorType\" = 'SonOffMiniR3Swich' WHERE \"SensorType\" = 'Swich'");
            migrationBuilder.Sql("UPDATE \"AlarmLookups\" SET \"SensorType\" = 'SonOffMiniR3Swich' WHERE \"SensorType\" = 'Swich'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Sensors\" SET \"Type\" = 'Swich' WHERE \"Type\" = 'SonOffMiniR3Swich'");
            migrationBuilder.Sql("UPDATE \"DeviceSensors\" SET \"SensorType\" = 'Swich' WHERE \"SensorType\" = 'SonOffMiniR3Swich'");
            migrationBuilder.Sql("UPDATE \"AlarmLookups\" SET \"SensorType\" = 'Swich' WHERE \"SensorType\" = 'SonOffMiniR3Swich'");
        }
    }
}
