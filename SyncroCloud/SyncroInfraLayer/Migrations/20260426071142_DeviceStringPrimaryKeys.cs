using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SyncroInfraLayer.Migrations
{
    /// <inheritdoc />
    public partial class DeviceStringPrimaryKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceApiKeys_Devices_DeviceId",
                table: "DeviceApiKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceScenarios_Devices_DeviceId",
                table: "DeviceScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceSensors_Devices_DeviceId",
                table: "DeviceSensors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Devices");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceSensors",
                type: "character varying(100)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Npgsql generates TYPE change before DROP IDENTITY, which PostgreSQL rejects.
            // Use raw SQL to enforce the correct order: drop identity first, then retype.
            migrationBuilder.Sql(@"
                ALTER TABLE ""DeviceSensors"" ALTER COLUMN ""Id"" DROP IDENTITY;
                ALTER TABLE ""DeviceSensors"" ALTER COLUMN ""Id"" TYPE character varying(500) USING ""Id""::varchar;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceScenarios",
                type: "character varying(100)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceSensorId",
                table: "DeviceReadings",
                type: "character varying(500)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceReadings",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "DeviceApiKeys",
                type: "character varying(100)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceApiKeys_Devices_DeviceId",
                table: "DeviceApiKeys",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceScenarios_Devices_DeviceId",
                table: "DeviceScenarios",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceSensors_Devices_DeviceId",
                table: "DeviceSensors",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings",
                column: "DeviceSensorId",
                principalTable: "DeviceSensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceReadings_DeviceSensors_DeviceSensorId",
                table: "DeviceReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceApiKeys_Devices_DeviceId",
                table: "DeviceApiKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceScenarios_Devices_DeviceId",
                table: "DeviceScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceSensors_Devices_DeviceId",
                table: "DeviceSensors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceSensors",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "DeviceSensors",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceScenarios",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Devices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<long>(
                name: "DeviceSensorId",
                table: "DeviceReadings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceReadings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceId",
                table: "DeviceApiKeys",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceId",
                table: "Devices",
                column: "DeviceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceApiKeys_Devices_DeviceId",
                table: "DeviceApiKeys",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceScenarios_Devices_DeviceId",
                table: "DeviceScenarios",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceSensors_Devices_DeviceId",
                table: "DeviceSensors",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
