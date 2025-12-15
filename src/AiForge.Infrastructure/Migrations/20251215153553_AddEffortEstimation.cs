using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEffortEstimation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EffortEstimations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Complexity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstimatedEffort = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfidencePercent = table.Column<int>(type: "int", nullable: false),
                    EstimationReasoning = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Assumptions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActualEffort = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VarianceNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    RevisionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffortEstimations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EffortEstimations_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EffortEstimations_TicketId",
                table: "EffortEstimations",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_EffortEstimations_TicketId_IsLatest",
                table: "EffortEstimations",
                columns: new[] { "TicketId", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_EffortEstimations_TicketId_Version",
                table: "EffortEstimations",
                columns: new[] { "TicketId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EffortEstimations");
        }
    }
}
