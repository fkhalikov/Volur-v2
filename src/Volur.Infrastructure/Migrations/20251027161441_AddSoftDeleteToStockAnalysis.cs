using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Volur.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToStockAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StockNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StockKeyValues",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockNotes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockKeyValues");
        }
    }
}
