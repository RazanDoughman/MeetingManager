using MeetingManager.Rooms.Model;
using MeetingManager.Rooms.DTO;

namespace MeetingManager.Rooms.Service
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room> GetRoomByIdAsync(Guid id);
        Task<Room> CreateRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Guid id, Room room);
        Task<bool> DeleteRoomAsync(Guid id);

        Task<DailyAvailabilityDto?> GetDailyAvailabilityAsync(
           Guid roomId,
           DateOnly localDate,
           int slotMinutes = 30,
           int startHourLocal = 9,
           int endHourLocal = 17);
    }
}