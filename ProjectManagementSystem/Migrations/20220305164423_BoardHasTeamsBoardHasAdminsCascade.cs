using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class BoardHasTeamsBoardHasAdminsCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardHasAdmins_AspNetUsers_user_id",
                table: "boardHasAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasAdmins_boards_board_id",
                table: "boardHasAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasTeams_teams_team_id",
                table: "boardHasTeams");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasAdmins_AspNetUsers_user_id",
                table: "boardHasAdmins",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasAdmins_boards_board_id",
                table: "boardHasAdmins",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasTeams_teams_team_id",
                table: "boardHasTeams",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boardHasAdmins_AspNetUsers_user_id",
                table: "boardHasAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasAdmins_boards_board_id",
                table: "boardHasAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_boardHasTeams_teams_team_id",
                table: "boardHasTeams");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasAdmins_AspNetUsers_user_id",
                table: "boardHasAdmins",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasAdmins_boards_board_id",
                table: "boardHasAdmins",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasTeams_boards_board_id",
                table: "boardHasTeams",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_boardHasTeams_teams_team_id",
                table: "boardHasTeams",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "Id");
        }
    }
}
