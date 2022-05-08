using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class ChangeSectionJobDeleteBehaviour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobs_sections_sectionId",
                table: "jobs");

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_sections_sectionId",
                table: "jobs",
                column: "sectionId",
                principalTable: "sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobs_sections_sectionId",
                table: "jobs");

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_sections_sectionId",
                table: "jobs",
                column: "sectionId",
                principalTable: "sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
