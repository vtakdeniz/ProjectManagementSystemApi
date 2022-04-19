using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class UpdateNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "boardId",
                table: "notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_boardId",
                table: "notifications",
                column: "boardId");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_boards_boardId",
                table: "notifications",
                column: "boardId",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_boards_boardId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_boardId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "boardId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "notifications");
        }
    }
}
