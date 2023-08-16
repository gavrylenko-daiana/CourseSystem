using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedCourseAndGroupToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CourseId",
                table: "Notifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Courses_CourseId",
                table: "Notifications",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Courses_CourseId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CourseId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Notifications");
        }
    }
}
