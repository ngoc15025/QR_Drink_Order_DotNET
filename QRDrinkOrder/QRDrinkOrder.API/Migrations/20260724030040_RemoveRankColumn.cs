using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDrinkOrder.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRankColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedPinAttempts",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PinCodeHash",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinLockoutEnd",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedPinAttempts",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "PinCodeHash",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "PinLockoutEnd",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuditLogs");
        }
    }
}
