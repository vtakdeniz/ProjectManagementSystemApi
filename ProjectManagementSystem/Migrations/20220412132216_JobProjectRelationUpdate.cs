using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class JobProjectRelationUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobs_projects_project_id",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_jobs_sections_section_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_project_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_section_id",
                table: "jobs");

            migrationBuilder.AddColumn<int>(
                name: "projectId",
                table: "jobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sectionId",
                table: "jobs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_jobs_projectId",
                table: "jobs",
                column: "projectId");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_sectionId",
                table: "jobs",
                column: "sectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_projects_projectId",
                table: "jobs",
                column: "projectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_sections_sectionId",
                table: "jobs",
                column: "sectionId",
                principalTable: "sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobs_projects_projectId",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_jobs_sections_sectionId",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_projectId",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_sectionId",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "projectId",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "sectionId",
                table: "jobs");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_project_id",
                table: "jobs",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_section_id",
                table: "jobs",
                column: "section_id");

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_projects_project_id",
                table: "jobs",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_sections_section_id",
                table: "jobs",
                column: "section_id",
                principalTable: "sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
