using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendeeIsOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(
                name: "IsOrganizer",
                table: "Attendee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Attendee_MeetingId_UserId",
                table: "Attendee",
                columns: new[] { "MeetingId", "UserId" });

            migrationBuilder.CreateIndex(
               name: "IX_Attendee_UserId",
               table: "Attendee",
               column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendee_MeetingId_UserId",
                table: "Attendee");

            migrationBuilder.DropIndex(
               name: "IX_Attendee_UserId",
               table: "Attendee");

            migrationBuilder.DropColumn(
                  name: "IsOrganizer",
                  table: "Attendee");
        }
    }
}
