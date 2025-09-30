public class CreateMeetingDto
{
    public Guid RoomId { get; set; }
    //public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Agenda { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; }
    public List<Guid> AttendeeUserIds { get; set; } = new();

}

