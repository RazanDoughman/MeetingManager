using System.ComponentModel.DataAnnotations;

namespace MeetingManager.Meetings.DTO
{
    public class RescheduleMeetingDto
    {
        [Required]
        public DateTime StartTime { get; set; } // Expecting UTC ISO from client

        [Required]
        public DateTime EndTime { get; set; }   // Expecting UTC ISO from client
    }
}
