using MeetingManager.Meetings.Model;
using MeetingManager.Users.Model;
using System;

namespace MeetingManager.Notes.Model
{
    public class Note
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string NoteBody { get; set; }
        public DateTime CreatedAt { get; set; }

        public Meeting Meeting { get; set; }
        public User CreatedByUser { get; set; }
    }
}
