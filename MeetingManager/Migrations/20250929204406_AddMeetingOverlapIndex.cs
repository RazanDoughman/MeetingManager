using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingOverlapIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_RoomId_StartTime_EndTime",
                table: "Meeting",
                columns: new[] { "RoomId", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meeting_RoomId_StartTime_EndTime",
                table: "Meeting");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_RoomId",
                table: "Meeting",
                column: "RoomId");
        }
    }
}
