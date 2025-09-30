using System;
using System.Collections.Generic;

namespace MeetingManager.Meetings.DTO
{
    public class UpcomingMeetingItemDto
    {
        public Guid MeetingId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string? Status { get; set; }

        public string? RoomName { get; set; }

        public Guid OrganizerId { get; set; }
        public string? OrganizerName { get; set; }
        public string? OrganizerEmail { get; set; }

        public string Scope { get; set; } = "mine";

        public List<MeetingAttendeeDto> Attendees { get; set; } = new();
    }

    public class MeetingAttendeeDto
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Rsvp { get; set; }   // if you store RSVP/Status per attendee
    }
}
