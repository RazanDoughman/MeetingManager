using MeetingManager.Rooms.Model;
using MeetingManager.Users.Model;
using System;

namespace MeetingManager.Meetings.Model
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Room Room { get; set; }
        public User User { get; set; }
    }
}
