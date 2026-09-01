using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StabilizedArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_predictions_Matches_MatchId",
                table: "predictions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_predictions",
                table: "predictions");

            migrationBuilder.DropIndex(
                name: "IX_predictions_ExpectedValue",
                table: "predictions");

            migrationBuilder.DropIndex(
                name: "IX_predictions_Probability",
                table: "predictions");

            migrationBuilder.DropIndex(
                name: "IX_predictions_StrategyVersion",
                table: "predictions");

            migrationBuilder.DropColumn(
                name: "ExpectedValue",
                table: "predictions");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "predictions");

            migrationBuilder.DropColumn(
                name: "OddsAtPrediction",
                table: "predictions");

            migrationBuilder.DropColumn(
                name: "PredictedResult",
                table: "predictions");

            migrationBuilder.DropColumn(
                name: "Probability",
                table: "predictions");

            migrationBuilder.RenameTable(
                name: "predictions",
                newName: "Predictions");

            migrationBuilder.RenameColumn(
                name: "confidence",
                table: "Predictions",
                newName: "Confidence");

            migrationBuilder.RenameIndex(
                name: "IX_predictions_MatchId",
                table: "Predictions",
                newName: "IX_Predictions_MatchId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Confidence",
                table: "Predictions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "PredictionType",
                table: "Predictions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Predictions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PredictedOutcome",
                table: "Predictions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Predictions",
                table: "Predictions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SmartMoneyTipsters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ROI = table.Column<decimal>(type: "numeric", nullable: false),
                    WinRate = table.Column<decimal>(type: "numeric", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalBets = table.Column<int>(type: "integer", nullable: false),
                    IsTrusted = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartMoneyTipsters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmartMoneyPicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TipsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeam = table.Column<string>(type: "text", nullable: false),
                    AwayTeam = table.Column<string>(type: "text", nullable: false),
                    PredictionType = table.Column<string>(type: "text", nullable: false),
                    PredictionOutcome = table.Column<string>(type: "text", nullable: false),
                    Odds = table.Column<decimal>(type: "numeric", nullable: false),
                    MatchDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartMoneyPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmartMoneyPicks_SmartMoneyTipsters_TipsterId",
                        column: x => x.TipsterId,
                        principalTable: "SmartMoneyTipsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmartMoneyPicks_TipsterId",
                table: "SmartMoneyPicks",
                column: "TipsterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Predictions_Matches_MatchId",
                table: "Predictions",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predictions_Matches_MatchId",
                table: "Predictions");

            migrationBuilder.DropTable(
                name: "SmartMoneyPicks");

            migrationBuilder.DropTable(
                name: "SmartMoneyTipsters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Predictions",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "PredictedOutcome",
                table: "Predictions");

            migrationBuilder.RenameTable(
                name: "Predictions",
                newName: "predictions");

            migrationBuilder.RenameColumn(
                name: "Confidence",
                table: "predictions",
                newName: "confidence");

            migrationBuilder.RenameIndex(
                name: "IX_Predictions_MatchId",
                table: "predictions",
                newName: "IX_predictions_MatchId");

            migrationBuilder.AlterColumn<int>(
                name: "PredictionType",
                table: "predictions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "confidence",
                table: "predictions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedValue",
                table: "predictions",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "predictions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OddsAtPrediction",
                table: "predictions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PredictedResult",
                table: "predictions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Probability",
                table: "predictions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_predictions",
                table: "predictions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_ExpectedValue",
                table: "predictions",
                column: "ExpectedValue");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_Probability",
                table: "predictions",
                column: "Probability");

            migrationBuilder.CreateIndex(
                name: "IX_predictions_StrategyVersion",
                table: "predictions",
                column: "StrategyVersion");

            migrationBuilder.AddForeignKey(
                name: "FK_predictions_Matches_MatchId",
                table: "predictions",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
