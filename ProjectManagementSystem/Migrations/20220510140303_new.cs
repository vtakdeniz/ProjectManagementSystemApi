using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class @new : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id");
        }
    }
}
