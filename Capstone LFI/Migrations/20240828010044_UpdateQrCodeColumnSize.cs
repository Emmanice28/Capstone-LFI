using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_LFI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQrCodeColumnSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeBase64",
                table: "Inventory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeBase64",
                table: "Inventory");
        }
    }
}
