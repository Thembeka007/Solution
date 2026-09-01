using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionStrategyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageConfidence",
                table: "StrategyPerformances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceAccuracyGap",
                table: "StrategyPerformances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "StrategyPerformances",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IncorrectPredictions",
                table: "StrategyPerformances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StrategyPerformances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEvaluationUtc",
                table: "StrategyPerformances",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "ROI",
                table: "StrategyPerformances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ReplacementTriggered",
                table: "StrategyPerformances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "StrategyPerformances",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageConfidence",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "ConfidenceAccuracyGap",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "IncorrectPredictions",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "LastEvaluationUtc",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "ROI",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "ReplacementTriggered",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "StrategyPerformances");
        }
    }
}
