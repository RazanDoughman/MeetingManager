using MeetingManager;
using MeetingManager.Attachments.Model;
using MeetingManager;
using Microsoft.EntityFrameworkCore;

public class AttachmentService : IAttachmentService
{
    private readonly AppDbContext _context;

    public AttachmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Attachment>> GetAllAsync()
    {
        return await _context.Attachments
            .Include(a => a.Meeting)
            .ToListAsync();
    }

    public async Task<Attachment?> GetByIdAsync(Guid id)
    {
        return await _context.Attachments
            .Include(a => a.Meeting)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Attachment> CreateAsync(Attachment attachment)
    {
        attachment.Id = Guid.NewGuid();
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null) return false;

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return true;
    }
}
