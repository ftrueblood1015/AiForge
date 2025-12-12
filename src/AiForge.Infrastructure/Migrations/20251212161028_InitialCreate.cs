using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextTicketNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeyUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WindowStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeyUsages_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ParentTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentHandoffSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Tickets_ParentTicketId",
                        column: x => x.ParentTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HandoffDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StructuredContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SupersededById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoffDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandoffDocuments_HandoffDocuments_SupersededById",
                        column: x => x.SupersededById,
                        principalTable: "HandoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HandoffDocuments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InitialUnderstanding = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Assumptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternativesConsidered = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChosenApproach = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Rationale = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningSessions_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    FilesAffected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressEntries_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReasoningLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DecisionPoint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OptionsConsidered = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChosenOption = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    ConfidencePercent = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReasoningLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReasoningLogs_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistories_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HandoffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentBefore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentAfter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileSnapshots_HandoffDocuments_HandoffId",
                        column: x => x.HandoffId,
                        principalTable: "HandoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HandoffVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HandoffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StructuredContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoffVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandoffVersions_HandoffDocuments_HandoffId",
                        column: x => x.HandoffId,
                        principalTable: "HandoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_IsActive",
                table: "ApiKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyUsages_ApiKeyId_WindowStart",
                table: "ApiKeyUsages",
                columns: new[] { "ApiKeyId", "WindowStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyUsages_WindowStart",
                table: "ApiKeyUsages",
                column: "WindowStart");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TicketId",
                table: "Comments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_FileSnapshots_HandoffId",
                table: "FileSnapshots",
                column: "HandoffId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffDocuments_CreatedAt",
                table: "HandoffDocuments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffDocuments_IsActive",
                table: "HandoffDocuments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffDocuments_SessionId",
                table: "HandoffDocuments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffDocuments_SupersededById",
                table: "HandoffDocuments",
                column: "SupersededById");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffDocuments_TicketId",
                table: "HandoffDocuments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffVersions_HandoffId_Version",
                table: "HandoffVersions",
                columns: new[] { "HandoffId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningSessions_SessionId",
                table: "PlanningSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningSessions_TicketId",
                table: "PlanningSessions",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressEntries_CreatedAt",
                table: "ProgressEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressEntries_SessionId",
                table: "ProgressEntries",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressEntries_TicketId",
                table: "ProgressEntries",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Key",
                table: "Projects",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReasoningLogs_CreatedAt",
                table: "ReasoningLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReasoningLogs_SessionId",
                table: "ReasoningLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReasoningLogs_TicketId",
                table: "ReasoningLogs",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistories_ChangedAt",
                table: "TicketHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistories_TicketId",
                table: "TicketHistories",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Key",
                table: "Tickets",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ParentTicketId",
                table: "Tickets",
                column: "ParentTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProjectId_Number",
                table: "Tickets",
                columns: new[] { "ProjectId", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProjectId_Status",
                table: "Tickets",
                columns: new[] { "ProjectId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyUsages");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "FileSnapshots");

            migrationBuilder.DropTable(
                name: "HandoffVersions");

            migrationBuilder.DropTable(
                name: "PlanningSessions");

            migrationBuilder.DropTable(
                name: "ProgressEntries");

            migrationBuilder.DropTable(
                name: "ReasoningLogs");

            migrationBuilder.DropTable(
                name: "TicketHistories");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "HandoffDocuments");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
