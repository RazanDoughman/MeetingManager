public class ActionItemDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string MeetingTitle { get; set; }

    public string Description { get; set; }
    public bool ActionApproval { get; set; }

    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; }

    public Guid AssignedToId { get; set; }
    public string AssignedToName { get; set; }

    public DateTime ActionDeadline { get; set; }
    public string ActionStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}
