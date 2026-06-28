using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class RestructureDeviceScenarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old rows stored one scenario per row (Payload = single object); the new model
            // stores one row per device (Payload = full scenario array), so existing rows
            // can't be losslessly converted — clear them before restructuring.
            migrationBuilder.Sql("DELETE FROM \"DeviceScenarios\";");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceScenarios",
                table: "DeviceScenarios");

            migrationBuilder.DropIndex(
                name: "IX_DeviceScenarios_DeviceId",
                table: "DeviceScenarios");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DeviceScenarios");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceScenarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceScenarios",
                table: "DeviceScenarios",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceScenarios",
                table: "DeviceScenarios");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceScenarios",
                type: "character varying(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DeviceScenarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceScenarios",
                table: "DeviceScenarios",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceScenarios_DeviceId",
                table: "DeviceScenarios",
                column: "DeviceId");
        }
    }
}
