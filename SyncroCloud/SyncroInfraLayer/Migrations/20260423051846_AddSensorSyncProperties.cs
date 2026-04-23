using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorSyncProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EventChangeDelta",
                table: "Sensors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EventChangeSync",
                table: "Sensors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SyncPeriodicity",
                table: "Sensors",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventChangeDelta",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "EventChangeSync",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "SyncPeriodicity",
                table: "Sensors");
        }
    }
}
