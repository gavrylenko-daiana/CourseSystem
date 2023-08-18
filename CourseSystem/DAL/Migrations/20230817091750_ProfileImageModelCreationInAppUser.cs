using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ProfileImageModelCreationInAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProfileImages_AppUserId",
                table: "ProfileImages");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileImages_AppUserId",
                table: "ProfileImages",
                column: "AppUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProfileImages_AppUserId",
                table: "ProfileImages");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileImages_AppUserId",
                table: "ProfileImages",
                column: "AppUserId");
        }
    }
}
