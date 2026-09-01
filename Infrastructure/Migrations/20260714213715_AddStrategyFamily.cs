using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Generation",
                table: "StrategyPerformances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentStrategyId",
                table: "StrategyPerformances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrategyFamily",
                table: "StrategyPerformances",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Generation",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "ParentStrategyId",
                table: "StrategyPerformances");

            migrationBuilder.DropColumn(
                name: "StrategyFamily",
                table: "StrategyPerformances");
        }
    }
}
