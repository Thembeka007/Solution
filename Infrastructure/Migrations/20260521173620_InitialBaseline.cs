using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stake = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Odds = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PotentialPayout = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ActualPayout = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CashedOut = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TeamName = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    IsInjured = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    MinutesPlayedRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    TeamDependencyScore = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "strategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ROI = table.Column<decimal>(type: "numeric", nullable: false),
                    WinRate = table.Column<decimal>(type: "numeric", nullable: false),
                    Drawdown = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_strategies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "team_forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false),
                    BTTS = table.Column<bool>(type: "boolean", nullable: false),
                    Over25 = table.Column<bool>(type: "boolean", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    IsHomeGame = table.Column<bool>(type: "boolean", nullable: false),
                    XG = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    OpponentXG = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_forms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    League = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tipsters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WinRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ROI = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Profit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPredictions = table.Column<int>(type: "integer", nullable: false),
                    IsSmartMoney = table.Column<bool>(type: "boolean", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipsters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    KickoffUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TipsterPredictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TipsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PredictedOutcome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Odds = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipsterPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TipsterPredictions_Tipsters_TipsterId",
                        column: x => x.TipsterId,
                        principalTable: "Tipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "predictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionType = table.Column<int>(type: "integer", nullable: false),
                    PredictedResult = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    Probability = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ExpectedValue = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    OddsAtPrediction = table.Column<decimal>(type: "numeric", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    StrategyVersion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_predictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_predictions_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bets_Category",
                table: "bets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_bets_Won",
                table: "bets",
                column: "Won");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AwayTeamId",
                table: "Matches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeTeamId",
                table: "Matches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_ExpectedValue",
                table: "predictions",
                column: "ExpectedValue");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_MatchId",
                table: "predictions",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_Probability",
                table: "predictions",
                column: "Probability");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_StrategyVersion",
                table: "predictions",
                column: "StrategyVersion");

            migrationBuilder.CreateIndex(
                name: "IX_strategies_IsActive",
                table: "strategies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_strategies_ROI",
                table: "strategies",
                column: "ROI");

            migrationBuilder.CreateIndex(
                name: "IX_team_forms_BTTS",
                table: "team_forms",
                column: "BTTS");

            migrationBuilder.CreateIndex(
                name: "IX_team_forms_MatchDateUtc",
                table: "team_forms",
                column: "MatchDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_team_forms_Over25",
                table: "team_forms",
                column: "Over25");

            migrationBuilder.CreateIndex(
                name: "IX_team_forms_TeamId",
                table: "team_forms",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_team_forms_TeamId_MatchDateUtc",
                table: "team_forms",
                columns: new[] { "TeamId", "MatchDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TipsterPredictions_TipsterId",
                table: "TipsterPredictions",
                column: "TipsterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bets");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "predictions");

            migrationBuilder.DropTable(
                name: "strategies");

            migrationBuilder.DropTable(
                name: "team_forms");

            migrationBuilder.DropTable(
                name: "TipsterPredictions");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Tipsters");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
