using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketEdge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ClosingLineValue",
                table: "BettingSlipSelections",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ClosingOdds",
                table: "BettingSlipSelections",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningOdds",
                table: "BettingSlipSelections",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MarketEdges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelProbability = table.Column<decimal>(type: "numeric", nullable: false),
                    MarketProbability = table.Column<decimal>(type: "numeric", nullable: false),
                    EdgePercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    FairOdds = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketEdges_Predictions_PredictionId",
                        column: x => x.PredictionId,
                        principalTable: "Predictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketEdges_PredictionId",
                table: "MarketEdges",
                column: "PredictionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketEdges");

            migrationBuilder.DropColumn(
                name: "ClosingLineValue",
                table: "BettingSlipSelections");

            migrationBuilder.DropColumn(
                name: "ClosingOdds",
                table: "BettingSlipSelections");

            migrationBuilder.DropColumn(
                name: "OpeningOdds",
                table: "BettingSlipSelections");
        }
    }
}
