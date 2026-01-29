using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionCheckpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutionCheckpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    CheckpointData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionCheckpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecutionCheckpoints_SkillChainExecutions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalTable: "SkillChainExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExecutionCheckpoints_SkillChainLinks_LinkId",
                        column: x => x.LinkId,
                        principalTable: "SkillChainLinks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionCheckpoints_ExecutionId",
                table: "ExecutionCheckpoints",
                column: "ExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionCheckpoints_ExecutionId_Position",
                table: "ExecutionCheckpoints",
                columns: new[] { "ExecutionId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionCheckpoints_LinkId",
                table: "ExecutionCheckpoints",
                column: "LinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecutionCheckpoints");
        }
    }
}
