public class AttachmentDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string MeetingTitle { get; set; }
    public string FileName { get; set; }
    public string Link { get; set; }
}
