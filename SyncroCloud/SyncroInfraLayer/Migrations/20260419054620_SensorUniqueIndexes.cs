using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class SensorUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sensors_Name",
                table: "Sensors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_UnitType_Type",
                table: "Sensors",
                columns: new[] { "UnitType", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_Name",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_UnitType_Type",
                table: "Sensors");
        }
    }
}
