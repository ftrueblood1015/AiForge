using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentPhase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkingSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LastCheckpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionStates_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionStates_WorkQueues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "WorkQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionStates_CreatedAt",
                table: "SessionStates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStates_ExpiresAt",
                table: "SessionStates",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStates_QueueId",
                table: "SessionStates",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionStates_SessionId",
                table: "SessionStates",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionStates_TicketId",
                table: "SessionStates",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionStates");
        }
    }
}
