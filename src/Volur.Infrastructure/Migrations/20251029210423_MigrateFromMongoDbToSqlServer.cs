using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Volur.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateFromMongoDbToSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockNotes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockKeyValues");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StockNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StockKeyValues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatingMic = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "NoDataAvailable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExchangeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    FirstFailedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoDataAvailable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockFundamentals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sector = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CurrencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarketCap = table.Column<double>(type: "float", nullable: true),
                    EnterpriseValue = table.Column<double>(type: "float", nullable: true),
                    TrailingPE = table.Column<double>(type: "float", nullable: true),
                    ForwardPE = table.Column<double>(type: "float", nullable: true),
                    PEG = table.Column<double>(type: "float", nullable: true),
                    PriceToSales = table.Column<double>(type: "float", nullable: true),
                    PriceToBook = table.Column<double>(type: "float", nullable: true),
                    EnterpriseToRevenue = table.Column<double>(type: "float", nullable: true),
                    EnterpriseToEbitda = table.Column<double>(type: "float", nullable: true),
                    ProfitMargins = table.Column<double>(type: "float", nullable: true),
                    GrossMargins = table.Column<double>(type: "float", nullable: true),
                    OperatingMargins = table.Column<double>(type: "float", nullable: true),
                    ReturnOnAssets = table.Column<double>(type: "float", nullable: true),
                    ReturnOnEquity = table.Column<double>(type: "float", nullable: true),
                    Revenue = table.Column<double>(type: "float", nullable: true),
                    RevenuePerShare = table.Column<double>(type: "float", nullable: true),
                    QuarterlyRevenueGrowth = table.Column<double>(type: "float", nullable: true),
                    QuarterlyEarningsGrowth = table.Column<double>(type: "float", nullable: true),
                    TotalCash = table.Column<double>(type: "float", nullable: true),
                    TotalCashPerShare = table.Column<double>(type: "float", nullable: true),
                    TotalDebt = table.Column<double>(type: "float", nullable: true),
                    DebtToEquity = table.Column<double>(type: "float", nullable: true),
                    CurrentRatio = table.Column<double>(type: "float", nullable: true),
                    BookValue = table.Column<double>(type: "float", nullable: true),
                    PriceToBookValue = table.Column<double>(type: "float", nullable: true),
                    DividendRate = table.Column<double>(type: "float", nullable: true),
                    DividendYield = table.Column<double>(type: "float", nullable: true),
                    PayoutRatio = table.Column<double>(type: "float", nullable: true),
                    Beta = table.Column<double>(type: "float", nullable: true),
                    FiftyTwoWeekLow = table.Column<double>(type: "float", nullable: true),
                    FiftyTwoWeekHigh = table.Column<double>(type: "float", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockFundamentals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentPrice = table.Column<double>(type: "float", nullable: true),
                    PreviousClose = table.Column<double>(type: "float", nullable: true),
                    Change = table.Column<double>(type: "float", nullable: true),
                    ChangePercent = table.Column<double>(type: "float", nullable: true),
                    Open = table.Column<double>(type: "float", nullable: true),
                    High = table.Column<double>(type: "float", nullable: true),
                    Low = table.Column<double>(type: "float", nullable: true),
                    Volume = table.Column<double>(type: "float", nullable: true),
                    AverageVolume = table.Column<double>(type: "float", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockQuotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Symbols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExchangeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParentExchange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrailingPE = table.Column<double>(type: "float", nullable: true),
                    MarketCap = table.Column<double>(type: "float", nullable: true),
                    CurrentPrice = table.Column<double>(type: "float", nullable: true),
                    ChangePercent = table.Column<double>(type: "float", nullable: true),
                    DividendYield = table.Column<double>(type: "float", nullable: true),
                    Sector = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symbols", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoDataAvailable_ExchangeCode",
                table: "NoDataAvailable",
                column: "ExchangeCode");

            migrationBuilder.CreateIndex(
                name: "IX_NoDataAvailable_Ticker_ExchangeCode",
                table: "NoDataAvailable",
                columns: new[] { "Ticker", "ExchangeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockFundamentals_Ticker",
                table: "StockFundamentals",
                column: "Ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockQuotes_Ticker",
                table: "StockQuotes",
                column: "Ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_ExchangeCode",
                table: "Symbols",
                column: "ExchangeCode");

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_FullSymbol",
                table: "Symbols",
                column: "FullSymbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_ParentExchange",
                table: "Symbols",
                column: "ParentExchange");

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_Ticker",
                table: "Symbols",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_Type",
                table: "Symbols",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropTable(
                name: "NoDataAvailable");

            migrationBuilder.DropTable(
                name: "StockFundamentals");

            migrationBuilder.DropTable(
                name: "StockQuotes");

            migrationBuilder.DropTable(
                name: "Symbols");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StockNotes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StockKeyValues");

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
    }
}
