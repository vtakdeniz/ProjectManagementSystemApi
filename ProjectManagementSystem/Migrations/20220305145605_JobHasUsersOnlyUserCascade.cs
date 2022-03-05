using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class JobHasUsersOnlyUserCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_taskHasUsers_AspNetUsers_user_id",
                table: "taskHasUsers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
