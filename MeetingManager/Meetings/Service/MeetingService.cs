using MeetingManager;
using MeetingManager.Meetings.Model;
using Microsoft.EntityFrameworkCore;

public class MeetingService : IMeetingService
{
    private readonly AppDbContext _context;

    public MeetingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Meeting>> GetAllAsync()
    {
        return await _context.Meetings
            .Include(m => m.Room)
            .Include(m => m.User)
            .ToListAsync();
    }

    public async Task<Meeting?> GetByIdAsync(Guid id)
    {
        return await _context.Meetings
            .Include(m => m.Room)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Meeting> CreateAsync(Meeting meeting)
    {
        meeting.Id = Guid.NewGuid();
        meeting.CreatedAt = DateTime.UtcNow;
        meeting.UpdatedAt = DateTime.UtcNow;

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();
        return meeting;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting == null) return false;

        _context.Meetings.Remove(meeting);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateMeetingDto dto)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting == null) return false;

        meeting.Title = dto.Title;
        meeting.Agenda = dto.Agenda;
        meeting.StartTime = dto.StartTime;
        meeting.EndTime = dto.EndTime;
        meeting.Status = dto.Status;
        meeting.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
