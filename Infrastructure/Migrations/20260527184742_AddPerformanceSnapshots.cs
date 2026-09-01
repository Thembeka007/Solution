using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ClosingOdds",
                table: "Predictions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "Predictions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "Predictions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfitLoss",
                table: "Predictions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledUtc",
                table: "Predictions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeagueName",
                table: "Matches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BetResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    Odds = table.Column<decimal>(type: "numeric", nullable: false),
                    Stake = table.Column<decimal>(type: "numeric", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    SettledUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    StrategyVersion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BetResults_Predictions_PredictionId",
                        column: x => x.PredictionId,
                        principalTable: "Predictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BettingSlips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Stake = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedOdds = table.Column<decimal>(type: "numeric", nullable: false),
                    PotentialPayout = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualPayout = table.Column<decimal>(type: "numeric", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    Settled = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    StrategyVersion = table.Column<string>(type: "text", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BettingSlips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "numeric", nullable: false),
                    ROI = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSlips = table.Column<int>(type: "integer", nullable: false),
                    WonSlips = table.Column<int>(type: "integer", nullable: false),
                    LostSlips = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<decimal>(type: "numeric", nullable: false),
                    WinningStreak = table.Column<int>(type: "integer", nullable: false),
                    LosingStreak = table.Column<int>(type: "integer", nullable: false),
                    BreadROI = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageROI = table.Column<decimal>(type: "numeric", nullable: false),
                    LotteryROI = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageConfidence = table.Column<decimal>(type: "numeric", nullable: false),
                    ConfidenceAccuracy = table.Column<decimal>(type: "numeric", nullable: false),
                    SmartMoneyAccuracy = table.Column<decimal>(type: "numeric", nullable: false),
                    TrueFormAccuracy = table.Column<decimal>(type: "numeric", nullable: false),
                    StrategyReplacementTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveStrategyVersion = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BettingSlipSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BettingSlipId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionType = table.Column<string>(type: "text", nullable: false),
                    PredictedOutcome = table.Column<string>(type: "text", nullable: false),
                    Odds = table.Column<decimal>(type: "numeric", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    Settled = table.Column<bool>(type: "boolean", nullable: false),
                    SettledUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    ProfitLoss = table.Column<decimal>(type: "numeric", nullable: false),
                    CashoutTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    CashoutAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    SmartMoneyStrength = table.Column<decimal>(type: "numeric", nullable: false),
                    TrueFormStrength = table.Column<decimal>(type: "numeric", nullable: false),
                    FinalModelConfidence = table.Column<decimal>(type: "numeric", nullable: false),
                    StrategyVersion = table.Column<string>(type: "text", nullable: false),
                    TrapDetected = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: false),
                    BettingCategory = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BettingSlipSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BettingSlipSelections_BettingSlips_BettingSlipId",
                        column: x => x.BettingSlipId,
                        principalTable: "BettingSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BettingSlipSelections_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetResults_PredictionId",
                table: "BetResults",
                column: "PredictionId");

            migrationBuilder.CreateIndex(
                name: "IX_BettingSlipSelections_BettingSlipId",
                table: "BettingSlipSelections",
                column: "BettingSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_BettingSlipSelections_MatchId",
                table: "BettingSlipSelections",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetResults");

            migrationBuilder.DropTable(
                name: "BettingSlipSelections");

            migrationBuilder.DropTable(
                name: "PerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "BettingSlips");

            migrationBuilder.DropColumn(
                name: "ClosingOdds",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "ProfitLoss",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "SettledUtc",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "LeagueName",
                table: "Matches");
        }
    }
}
