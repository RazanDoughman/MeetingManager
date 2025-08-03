using MeetingManager;
using MeetingManager.Notes.Model;
using Microsoft.EntityFrameworkCore;

public class NoteService : INoteService
{
    private readonly AppDbContext _context;

    public NoteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        return await _context.Notes
            .Include(n => n.CreatedByUser)
            .Include(n => n.Meeting)
            .ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await _context.Notes
            .Include(n => n.CreatedByUser)
            .Include(n => n.Meeting)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Note> CreateAsync(Note note)
    {
        note.Id = Guid.NewGuid();
        note.CreatedAt = DateTime.UtcNow;
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return false;

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }
}
