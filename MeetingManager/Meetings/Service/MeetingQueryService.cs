using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetingManager;                 
using MeetingManager.Meetings.DTO;

namespace MeetingManager.Service
{
    public class MeetingQueryService
    {
        private readonly AppDbContext _db;

        public MeetingQueryService(AppDbContext db) => _db = db;

        public async Task<(IReadOnlyList<UpcomingMeetingItemDto> items, int total)>
            GetUpcomingAsync(Guid domainUserId, string scope, int page, int pageSize, CancellationToken ct)
        {
            // interpret "canceled": keep it simple—anything not "Canceled" counts as upcoming if future
            var nowUtc = DateTime.UtcNow;

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<UpcomingMeetingItemDto> q = scope switch
            {
                "mine" => _db.Meetings
                    .AsNoTracking()
                    .Where(m => m.UserId == domainUserId
                             && m.StartTime >= nowUtc
                             && (m.Status == null || m.Status != "Canceled"))
                    .OrderBy(m => m.StartTime)
                    .Select(m => new UpcomingMeetingItemDto
                    {
                        MeetingId = m.Id,
                        Title = m.Title,
                        Description = m.Agenda,       
                        StartTime = m.StartTime,
                        EndTime = m.EndTime,
                        Status = m.Status,
                        RoomName = m.Room != null ? m.Room.RoomName : null,
                        OrganizerId = m.UserId,
                        OrganizerName = m.User != null ? m.User.Name : null,
                        OrganizerEmail = m.User != null ? m.User.Email : null,
                        Scope = "mine",
                        Attendees = new List<MeetingAttendeeDto>() 
                    }),

                "invited" => _db.Attendees
                    .AsNoTracking()
                    .Where(a => a.UserId == domainUserId
                             && a.IsOrganizer == false
                             && a.Meeting.StartTime >= nowUtc
                             && (a.Meeting.Status == null || a.Meeting.Status != "Canceled"))
                    .OrderBy(a => a.Meeting.StartTime)
                    .Select(a => new UpcomingMeetingItemDto
                    {
                        MeetingId = a.MeetingId,
                        Title = a.Meeting.Title,
                        Description = a.Meeting.Agenda, 
                        StartTime = a.Meeting.StartTime,
                        EndTime = a.Meeting.EndTime,
                        Status = a.Meeting.Status,
                        RoomName = a.Meeting.Room != null ? a.Meeting.Room.RoomName : null,
                        OrganizerId = a.Meeting.UserId,
                        OrganizerName = a.Meeting.User != null ? a.Meeting.User.Name : null,
                        OrganizerEmail = a.Meeting.User != null ? a.Meeting.User.Email : null,
                        Scope = "invited",
                        Attendees = new List<MeetingAttendeeDto>() 
                    }),

                _ => throw new ArgumentException("scope must be 'mine' or 'invited'")
            };

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

            return (items, total);
        }
    }
}
