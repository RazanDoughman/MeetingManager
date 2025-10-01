using MeetingManager.Rooms.Model;
using Microsoft.EntityFrameworkCore;
using MeetingManager.Rooms.DTO;

namespace MeetingManager.Rooms.Service
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms.ToListAsync();
        }

        public async Task<Room> GetRoomByIdAsync(Guid id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<bool> UpdateRoomAsync(Guid id, Room room)
        {
            if (id != room.Id)
                return false;

            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoomAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DailyAvailabilityDto?> GetDailyAvailabilityAsync(
            Guid roomId,
            DateOnly localDate,
            int slotMinutes = 30,
            int startHourLocal = 9,
            int endHourLocal = 17)
        {
            
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");

            var room = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roomId);
            if (room is null) return null;

           
            var startLocal = new DateTime(localDate.Year, localDate.Month, localDate.Day, startHourLocal, 0, 0, DateTimeKind.Unspecified);
            var endLocal = new DateTime(localDate.Year, localDate.Month, localDate.Day, endHourLocal, 0, 0, DateTimeKind.Unspecified);

            
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

            
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Where(m => m.RoomId == roomId &&
                            m.StartTime < endUtc &&
                            m.EndTime > startUtc &&
                            m.Status != "Canceled")
                .Select(m => new { m.Id, m.Title, m.StartTime, m.EndTime })
                .ToListAsync();

            var dto = new DailyAvailabilityDto
            {
                RoomId = room.Id,
                RoomName = room.RoomName,
                Date = localDate,
                SlotMinutes = slotMinutes,
                StartHourLocal = startHourLocal,
                EndHourLocal = endHourLocal
            };

            var cursor = startLocal;
            while (cursor < endLocal)
            {
                var slotStartLocal = cursor;
                var slotEndLocal = cursor.AddMinutes(slotMinutes);

                var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartLocal, tz);
                var slotEndUtc = TimeZoneInfo.ConvertTimeToUtc(slotEndLocal, tz);

                var hit = meetings.FirstOrDefault(m => m.StartTime < slotEndUtc && m.EndTime > slotStartUtc);

                dto.Slots.Add(new AvailabilitySlotDto
                {
                    StartLocal = slotStartLocal,
                    EndLocal = slotEndLocal,
                    IsBooked = hit != null,
                    MeetingTitle = hit?.Title,
                    MeetingId = hit?.Id
                });

                cursor = slotEndLocal;
            }

            return dto;
        }

    }
}
