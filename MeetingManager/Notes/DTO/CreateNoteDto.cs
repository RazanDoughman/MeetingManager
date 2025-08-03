public class CreateNoteDto
{
    public Guid MeetingId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string NoteBody { get; set; }
}
