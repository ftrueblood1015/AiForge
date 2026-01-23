using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentSkillConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Capabilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.CheckConstraint("CK_Agent_Scope", "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Agents_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AgentIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkillIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSets", x => x.Id);
                    table.CheckConstraint("CK_ConfigurationSet_Scope", "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ConfigurationSets_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                    table.CheckConstraint("CK_PromptTemplate_Scope", "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PromptTemplates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.CheckConstraint("CK_Skill_Scope", "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Skills_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AgentKey",
                table: "Agents",
                column: "AgentKey");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AgentKey_OrganizationId_ProjectId",
                table: "Agents",
                columns: new[] { "AgentKey", "OrganizationId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_IsEnabled",
                table: "Agents",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_OrganizationId",
                table: "Agents",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ProjectId",
                table: "Agents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Status",
                table: "Agents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSets_OrganizationId",
                table: "ConfigurationSets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSets_ProjectId",
                table: "ConfigurationSets",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSets_SetKey",
                table: "ConfigurationSets",
                column: "SetKey");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSets_SetKey_OrganizationId_ProjectId",
                table: "ConfigurationSets",
                columns: new[] { "SetKey", "OrganizationId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Category",
                table: "PromptTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_OrganizationId",
                table: "PromptTemplates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_ProjectId",
                table: "PromptTemplates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_TemplateKey",
                table: "PromptTemplates",
                column: "TemplateKey");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_TemplateKey_OrganizationId_ProjectId",
                table: "PromptTemplates",
                columns: new[] { "TemplateKey", "OrganizationId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Category",
                table: "Skills",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_IsPublished",
                table: "Skills",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_OrganizationId",
                table: "Skills",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_ProjectId",
                table: "Skills",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillKey",
                table: "Skills",
                column: "SkillKey");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillKey_OrganizationId_ProjectId",
                table: "Skills",
                columns: new[] { "SkillKey", "OrganizationId", "ProjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "ConfigurationSets");

            migrationBuilder.DropTable(
                name: "PromptTemplates");

            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
