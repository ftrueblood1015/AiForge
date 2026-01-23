using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkQueueEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImplementationPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CheckedOutBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckoutExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkQueues_HandoffDocuments_ImplementationPlanId",
                        column: x => x.ImplementationPlanId,
                        principalTable: "HandoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkQueues_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkQueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkQueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkQueueItems_WorkQueues_WorkQueueId",
                        column: x => x.WorkQueueId,
                        principalTable: "WorkQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueueItems_WorkItemId_WorkItemType",
                table: "WorkQueueItems",
                columns: new[] { "WorkItemId", "WorkItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueueItems_WorkQueueId_Position",
                table: "WorkQueueItems",
                columns: new[] { "WorkQueueId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueueItems_WorkQueueId_WorkItemId_WorkItemType",
                table: "WorkQueueItems",
                columns: new[] { "WorkQueueId", "WorkItemId", "WorkItemType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueues_CheckedOutBy",
                table: "WorkQueues",
                column: "CheckedOutBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueues_ImplementationPlanId",
                table: "WorkQueues",
                column: "ImplementationPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueues_ProjectId",
                table: "WorkQueues",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueues_Status",
                table: "WorkQueues",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkQueueItems");

            migrationBuilder.DropTable(
                name: "WorkQueues");
        }
    }
}
