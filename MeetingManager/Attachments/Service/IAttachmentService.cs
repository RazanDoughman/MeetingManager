using MeetingManager.Attachments.Model;

public interface IAttachmentService
{
    Task<List<Attachment>> GetAllAsync();
    Task<Attachment?> GetByIdAsync(Guid id);
    Task<Attachment> CreateAsync(Attachment attachment);
    Task<bool> DeleteAsync(Guid id);
}
