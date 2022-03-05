using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class BoardHasUsersCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardHasUsers_AspNetUsers_user_id",
                table: "boardHasUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasUsers_boards_board_id",
                table: "boardHasUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasUsers_AspNetUsers_user_id",
                table: "boardHasUsers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasUsers_boards_board_id",
                table: "boardHasUsers",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardHasUsers_AspNetUsers_user_id",
                table: "boardHasUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasUsers_boards_board_id",
                table: "boardHasUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasUsers_AspNetUsers_user_id",
                table: "boardHasUsers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasUsers_boards_board_id",
                table: "boardHasUsers",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id");
        }
    }
}
