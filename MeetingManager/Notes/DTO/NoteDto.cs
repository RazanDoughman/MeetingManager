public class NoteDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string NoteBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; }
}
