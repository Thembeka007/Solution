using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMutationOutcomeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MutationOutcomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyVersion = table.Column<string>(type: "text", nullable: false),
                    ParameterName = table.Column<string>(type: "text", nullable: false),
                    OldValue = table.Column<decimal>(type: "numeric", nullable: false),
                    NewValue = table.Column<decimal>(type: "numeric", nullable: false),
                    PreviousROI = table.Column<decimal>(type: "numeric", nullable: false),
                    NewROI = table.Column<decimal>(type: "numeric", nullable: false),
                    Improvement = table.Column<decimal>(type: "numeric", nullable: false),
                    Successful = table.Column<bool>(type: "boolean", nullable: false),
                    MutationUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EvaluatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutationOutcomes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MutationOutcomes");
        }
    }
}
