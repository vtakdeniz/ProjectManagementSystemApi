using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class CascadeDeleteTeamHasUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_taskHasUsers_jobs_job_id",
                table: "taskHasUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_taskHasUsers_jobs_job_id",
                table: "taskHasUsers",
                column: "job_id",
                principalTable: "jobs",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_taskHasUsers_jobs_job_id",
                table: "taskHasUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_taskHasUsers_jobs_job_id",
                table: "taskHasUsers",
                column: "job_id",
                principalTable: "jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
