using MeetingManager;
using MeetingManager.Meetings.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

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
        
        meeting.StartTime = DateTime.SpecifyKind(meeting.StartTime, DateTimeKind.Utc);
        meeting.EndTime = DateTime.SpecifyKind(meeting.EndTime, DateTimeKind.Utc);

        if (meeting.EndTime <= meeting.StartTime)
            throw new InvalidOperationException("End time must be after start time.");

      
        await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        
        bool overlaps = await _context.Meetings.AnyAsync(m =>
            m.RoomId == meeting.RoomId &&
            meeting.StartTime < m.EndTime &&
            meeting.EndTime > m.StartTime
        );

        if (overlaps)
            throw new InvalidOperationException("Room is already booked for the selected time.");

        meeting.Id = Guid.NewGuid();
        meeting.CreatedAt = DateTime.UtcNow;
        meeting.UpdatedAt = DateTime.UtcNow;

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        await tx.CommitAsync();

        return meeting;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var meeting = await _context.Meetings
       .Include(m => m.Room)
       .Include(m => m.User)
       .FirstOrDefaultAsync(m => m.Id == id);

        if (meeting == null) return false;

        // Remove all related attendees first
        var attendees = _context.Attendees.Where(a => a.MeetingId == id);
        _context.Attendees.RemoveRange(attendees);

        // Then remove the meeting
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

    public async Task<Meeting?> RescheduleAsync(Guid meetingId, DateTime newStartUtc, DateTime newEndUtc)
    {
        // Basic validation
        if (newEndUtc <= newStartUtc)
            throw new InvalidOperationException("End time must be after start time.");

        var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId);
        if (meeting is null) return null;

        // Cannot reschedule canceled/past meetings
        if (string.Equals(meeting.Status, "Canceled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Canceled meetings cannot be rescheduled.");
        if (meeting.EndTime < DateTime.UtcNow)
            throw new InvalidOperationException("Past meetings cannot be rescheduled.");

        // Double-booking check (same room, different meeting)
        var overlaps = await _context.Meetings
            .AsNoTracking()
            .AnyAsync(m =>
                m.RoomId == meeting.RoomId &&
                m.Id != meeting.Id &&
                m.Status != "Canceled" &&
                m.StartTime < newEndUtc &&
                m.EndTime > newStartUtc);

        if (overlaps)
            throw new ApplicationException("Room is already booked for the selected time.");

        // Update + save
        meeting.StartTime = newStartUtc;
        meeting.EndTime = newEndUtc;
        meeting.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return meeting;
    }

    public async Task<bool> CancelAsync(Guid meetingId, Guid currentUserId, bool isAdmin)
    {
        var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId);
        if (meeting == null) return false;

        // Only organizer or Admin can cancel
        if (meeting.UserId != currentUserId && !isAdmin)
            throw new UnauthorizedAccessException("You are not allowed to cancel this meeting.");

        // Already canceled?
        if (string.Equals(meeting.Status, "Canceled", StringComparison.OrdinalIgnoreCase))
            return false;

        meeting.Status = "Canceled";
        meeting.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

}

