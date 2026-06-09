using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDrinkOrder.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Memberships");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "Memberships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Đồng");
        }
    }
}
