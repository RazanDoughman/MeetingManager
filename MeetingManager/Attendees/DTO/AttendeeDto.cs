public class AttendeeDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string MeetingTitle { get; set; }

    public Guid UserId { get; set; }
    public string UserName { get; set; }

    public bool Attended { get; set; }
}
