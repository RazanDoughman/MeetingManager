public class MeetingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Agenda { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; }

    public Guid RoomId { get; set; }
    public string RoomName { get; set; }

    public Guid UserId { get; set; }
    public string UserName { get; set; }
}
