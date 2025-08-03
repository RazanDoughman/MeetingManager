using MeetingManager.Meetings.Model;
using System;

namespace MeetingManager.Attachments.Model
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }

        public Meeting Meeting { get; set; }
    }
}
