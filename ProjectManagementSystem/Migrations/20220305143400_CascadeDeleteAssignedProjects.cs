using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class CascadeDeleteAssignedProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userAssignedProjects_AspNetUsers_receiver_id",
                table: "userAssignedProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_userAssignedProjects_projects_project_id",
                table: "userAssignedProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_userAssignedProjects_AspNetUsers_receiver_id",
                table: "userAssignedProjects",
                column: "receiver_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userAssignedProjects_projects_project_id",
                table: "userAssignedProjects",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userAssignedProjects_AspNetUsers_receiver_id",
                table: "userAssignedProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_userAssignedProjects_projects_project_id",
                table: "userAssignedProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_userAssignedProjects_AspNetUsers_receiver_id",
                table: "userAssignedProjects",
                column: "receiver_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_userAssignedProjects_projects_project_id",
                table: "userAssignedProjects",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "Id");
        }
    }
}
