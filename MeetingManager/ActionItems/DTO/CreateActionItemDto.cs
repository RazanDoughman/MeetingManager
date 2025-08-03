public class CreateActionItemDto
{
    public Guid MeetingId { get; set; }
    public string Description { get; set; }
    public bool ActionApproval { get; set; }
    public Guid CreatedById { get; set; }
    public Guid AssignedToId { get; set; }
    public DateTime ActionDeadline { get; set; }
    public string ActionStatus { get; set; }
}
