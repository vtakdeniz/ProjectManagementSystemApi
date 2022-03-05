using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectManagementSystem.Migrations
{
    public partial class AddedNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    action_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    owner_user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sender_user_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sender_userId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    projectId = table.Column<int>(type: "int", nullable: true),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    jobId = table.Column<int>(type: "int", nullable: true),
                    job_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_AspNetUsers_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_AspNetUsers_sender_userId",
                        column: x => x.sender_userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_jobs_jobId",
                        column: x => x.jobId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_projects_projectId",
                        column: x => x.projectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_jobId",
                table: "notifications",
                column: "jobId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_owner_user_id",
                table: "notifications",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_projectId",
                table: "notifications",
                column: "projectId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_sender_userId",
                table: "notifications",
                column: "sender_userId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
