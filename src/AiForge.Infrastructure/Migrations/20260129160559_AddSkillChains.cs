using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillChains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkillChains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChainKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InputSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxTotalFailures = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillChains", x => x.Id);
                    table.CheckConstraint("CK_SkillChain_Scope", "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SkillChains_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillChainLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillChainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    OnSuccessTransition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OnSuccessTargetLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OnFailureTransition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OnFailureTargetLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkConfig = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillChainLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillChainLinks_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillChainLinks_SkillChainLinks_OnFailureTargetLinkId",
                        column: x => x.OnFailureTargetLinkId,
                        principalTable: "SkillChainLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillChainLinks_SkillChainLinks_OnSuccessTargetLinkId",
                        column: x => x.OnSuccessTargetLinkId,
                        principalTable: "SkillChainLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillChainLinks_SkillChains_SkillChainId",
                        column: x => x.SkillChainId,
                        principalTable: "SkillChains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillChainLinks_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillChainExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillChainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InputValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalFailureCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RequiresHumanIntervention = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InterventionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillChainExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillChainExecutions_SkillChainLinks_CurrentLinkId",
                        column: x => x.CurrentLinkId,
                        principalTable: "SkillChainLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillChainExecutions_SkillChains_SkillChainId",
                        column: x => x.SkillChainId,
                        principalTable: "SkillChains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillChainExecutions_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SkillChainLinkExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillChainExecutionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillChainLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Input = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransitionTaken = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillChainLinkExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillChainLinkExecutions_SkillChainExecutions_SkillChainExecutionId",
                        column: x => x.SkillChainExecutionId,
                        principalTable: "SkillChainExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillChainLinkExecutions_SkillChainLinks_SkillChainLinkId",
                        column: x => x.SkillChainLinkId,
                        principalTable: "SkillChainLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainExecutions_CurrentLinkId",
                table: "SkillChainExecutions",
                column: "CurrentLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainExecutions_RequiresHumanIntervention",
                table: "SkillChainExecutions",
                column: "RequiresHumanIntervention");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainExecutions_SkillChainId",
                table: "SkillChainExecutions",
                column: "SkillChainId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainExecutions_Status",
                table: "SkillChainExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainExecutions_TicketId",
                table: "SkillChainExecutions",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinkExecutions_Outcome",
                table: "SkillChainLinkExecutions",
                column: "Outcome");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinkExecutions_SkillChainExecutionId",
                table: "SkillChainLinkExecutions",
                column: "SkillChainExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinkExecutions_SkillChainExecutionId_SkillChainLinkId_AttemptNumber",
                table: "SkillChainLinkExecutions",
                columns: new[] { "SkillChainExecutionId", "SkillChainLinkId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinkExecutions_SkillChainLinkId",
                table: "SkillChainLinkExecutions",
                column: "SkillChainLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinks_AgentId",
                table: "SkillChainLinks",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinks_OnFailureTargetLinkId",
                table: "SkillChainLinks",
                column: "OnFailureTargetLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinks_OnSuccessTargetLinkId",
                table: "SkillChainLinks",
                column: "OnSuccessTargetLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinks_SkillChainId",
                table: "SkillChainLinks",
                column: "SkillChainId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinks_SkillChainId_Position",
                table: "SkillChainLinks",
                columns: new[] { "SkillChainId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillChainLinks_SkillId",
                table: "SkillChainLinks",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChains_ChainKey",
                table: "SkillChains",
                column: "ChainKey");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChains_ChainKey_OrganizationId_ProjectId",
                table: "SkillChains",
                columns: new[] { "ChainKey", "OrganizationId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillChains_IsPublished",
                table: "SkillChains",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChains_OrganizationId",
                table: "SkillChains",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChains_ProjectId",
                table: "SkillChains",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillChainLinkExecutions");

            migrationBuilder.DropTable(
                name: "SkillChainExecutions");

            migrationBuilder.DropTable(
                name: "SkillChainLinks");

            migrationBuilder.DropTable(
                name: "SkillChains");
        }
    }
}
