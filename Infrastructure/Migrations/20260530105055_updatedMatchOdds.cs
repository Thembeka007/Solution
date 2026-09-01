using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedMatchOdds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AwayImpliedProbability",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BTTSNoOdds",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BTTSYesOdds",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DrawImpliedProbability",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HomeImpliedProbability",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Over25Odds",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Under25Odds",
                table: "MatchOdds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayImpliedProbability",
                table: "MatchOdds");

            migrationBuilder.DropColumn(
                name: "BTTSNoOdds",
                table: "MatchOdds");

            migrationBuilder.DropColumn(
                name: "BTTSYesOdds",
                table: "MatchOdds");

            migrationBuilder.DropColumn(
                name: "DrawImpliedProbability",
                table: "MatchOdds");

            migrationBuilder.DropColumn(
                name: "HomeImpliedProbability",
                table: "MatchOdds");

            migrationBuilder.DropColumn(
                name: "Over25Odds",
                table: "MatchOdds");

            migrationBuilder.DropColumn(
                name: "Under25Odds",
                table: "MatchOdds");
        }
    }
}
