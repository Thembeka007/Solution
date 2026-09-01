using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingMutations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingMutations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyVersion = table.Column<string>(type: "text", nullable: false),
                    ParameterName = table.Column<string>(type: "text", nullable: false),
                    OldValue = table.Column<decimal>(type: "numeric", nullable: false),
                    NewValue = table.Column<decimal>(type: "numeric", nullable: false),
                    StartingROI = table.Column<decimal>(type: "numeric", nullable: false),
                    AppliedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Evaluated = table.Column<bool>(type: "boolean", nullable: false),
                    EvaluatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingMutations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingMutations");
        }
    }
}
