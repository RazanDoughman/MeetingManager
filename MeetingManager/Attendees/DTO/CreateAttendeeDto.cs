public class CreateAttendeeDto
{
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public bool Attended { get; set; }
}
