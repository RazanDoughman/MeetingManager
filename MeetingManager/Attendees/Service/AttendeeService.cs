using MeetingManager;
using MeetingManager.Attendees.Model;
using MeetingManager.Data;
using Microsoft.EntityFrameworkCore;

public class AttendeeService : IAttendeeService
{
    private readonly AppDbContext _context;

    public AttendeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Attendee>> GetAllAsync()
    {
        return await _context.Attendees
            .Include(a => a.Meeting)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<Attendee?> GetByIdAsync(Guid id)
    {
        return await _context.Attendees
            .Include(a => a.Meeting)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Attendee> CreateAsync(Attendee attendee)
    {
        attendee.Id = Guid.NewGuid();
        _context.Attendees.Add(attendee);
        await _context.SaveChangesAsync();
        return attendee;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return false;

        _context.Attendees.Remove(attendee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAttendeeDto dto)
    {
        var attendee = await _context.Attendees.FindAsync(id);
        if (attendee == null) return false;

        attendee.Attended = dto.Attended;
        await _context.SaveChangesAsync();
        return true;
    }
}
