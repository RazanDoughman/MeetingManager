using MeetingManager.Meetings.Model;
using MeetingManager.Users.Model;
using System;

namespace MeetingManager.ActionItems.Model
{
    public class ActionItem
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public string Description { get; set; }
        public bool ActionApproval { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid AssignedToId { get; set; }
        public DateTime ActionDeadline { get; set; }
        public string ActionStatus { get; set; }

        public Meeting Meeting { get; set; }
        public User CreatedBy { get; set; }
        public User AssignedTo { get; set; }
    }
}
