using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class AddCascadeDeleteProj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userHasProjects_AspNetUsers_user_id",
                table: "userHasProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_userHasProjects_projects_project_id",
                table: "userHasProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_userHasProjects_AspNetUsers_user_id",
                table: "userHasProjects",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userHasProjects_projects_project_id",
                table: "userHasProjects",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userHasProjects_AspNetUsers_user_id",
                table: "userHasProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_userHasProjects_projects_project_id",
                table: "userHasProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_userHasProjects_AspNetUsers_user_id",
                table: "userHasProjects",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_userHasProjects_projects_project_id",
                table: "userHasProjects",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "Id");
        }
    }
}
