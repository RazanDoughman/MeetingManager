using MeetingManager.Meetings.Model;
using MeetingManager.Users.Model;
using System;

namespace MeetingManager.Attendees.Model
{
    public class Attendee
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public Guid UserId { get; set; }
        public bool Attended { get; set; }

        public Meeting Meeting { get; set; }
        public User User { get; set; }
    }
}
