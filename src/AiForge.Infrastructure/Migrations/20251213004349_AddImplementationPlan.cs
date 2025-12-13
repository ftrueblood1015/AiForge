using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImplementationPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImplementationPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EstimatedEffort = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AffectedFiles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependencyTicketIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupersededById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImplementationPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImplementationPlans_ImplementationPlans_SupersededById",
                        column: x => x.SupersededById,
                        principalTable: "ImplementationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImplementationPlans_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImplementationPlans_Status",
                table: "ImplementationPlans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImplementationPlans_SupersededById",
                table: "ImplementationPlans",
                column: "SupersededById");

            migrationBuilder.CreateIndex(
                name: "IX_ImplementationPlans_TicketId",
                table: "ImplementationPlans",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ImplementationPlans_TicketId_Version",
                table: "ImplementationPlans",
                columns: new[] { "TicketId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImplementationPlans");
        }
    }
}
