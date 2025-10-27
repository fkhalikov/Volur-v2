using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Volur.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialStockAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockKeyValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExchangeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockKeyValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExchangeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockNotes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockKeyValues_Ticker_ExchangeCode_Key",
                table: "StockKeyValues",
                columns: new[] { "Ticker", "ExchangeCode", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_StockNotes_Ticker_ExchangeCode",
                table: "StockNotes",
                columns: new[] { "Ticker", "ExchangeCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockKeyValues");

            migrationBuilder.DropTable(
                name: "StockNotes");
        }
    }
}
