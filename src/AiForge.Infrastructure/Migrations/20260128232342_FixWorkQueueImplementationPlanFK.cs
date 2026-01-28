using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkQueueImplementationPlanFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkQueues_HandoffDocuments_ImplementationPlanId",
                table: "WorkQueues");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkQueues_ImplementationPlans_ImplementationPlanId",
                table: "WorkQueues",
                column: "ImplementationPlanId",
                principalTable: "ImplementationPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkQueues_ImplementationPlans_ImplementationPlanId",
                table: "WorkQueues");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkQueues_HandoffDocuments_ImplementationPlanId",
                table: "WorkQueues",
                column: "ImplementationPlanId",
                principalTable: "HandoffDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
