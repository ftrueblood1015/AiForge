using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessionStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionEndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    InputTokens = table.Column<int>(type: "int", nullable: true),
                    OutputTokens = table.Column<int>(type: "int", nullable: true),
                    TotalTokens = table.Column<int>(type: "int", nullable: true),
                    DecisionsLogged = table.Column<int>(type: "int", nullable: false),
                    ProgressEntriesLogged = table.Column<int>(type: "int", nullable: false),
                    FilesModified = table.Column<int>(type: "int", nullable: false),
                    HandoffCreated = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionMetrics_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionMetrics_CreatedAt",
                table: "SessionMetrics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMetrics_SessionId",
                table: "SessionMetrics",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMetrics_SessionStartedAt",
                table: "SessionMetrics",
                column: "SessionStartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMetrics_TicketId",
                table: "SessionMetrics",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionMetrics");
        }
    }
}
