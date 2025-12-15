using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinesAdded = table.Column<int>(type: "int", nullable: true),
                    LinesRemoved = table.Column<int>(type: "int", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileChanges_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalDebts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginatingTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResolutionTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AffectedFiles = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalDebts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalDebts_Tickets_OriginatingTicketId",
                        column: x => x.OriginatingTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechnicalDebts_Tickets_ResolutionTicketId",
                        column: x => x.ResolutionTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TestedFunctionality = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LinkedFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestLinks_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileChanges_FilePath",
                table: "FileChanges",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_FileChanges_TicketId",
                table: "FileChanges",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_FileChanges_TicketId_FilePath",
                table: "FileChanges",
                columns: new[] { "TicketId", "FilePath" });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDebts_Category",
                table: "TechnicalDebts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDebts_OriginatingTicketId",
                table: "TechnicalDebts",
                column: "OriginatingTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDebts_ResolutionTicketId",
                table: "TechnicalDebts",
                column: "ResolutionTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDebts_Severity",
                table: "TechnicalDebts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDebts_Status",
                table: "TechnicalDebts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TestLinks_LinkedFilePath",
                table: "TestLinks",
                column: "LinkedFilePath");

            migrationBuilder.CreateIndex(
                name: "IX_TestLinks_TestFilePath",
                table: "TestLinks",
                column: "TestFilePath");

            migrationBuilder.CreateIndex(
                name: "IX_TestLinks_TicketId",
                table: "TestLinks",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileChanges");

            migrationBuilder.DropTable(
                name: "TechnicalDebts");

            migrationBuilder.DropTable(
                name: "TestLinks");
        }
    }
}
