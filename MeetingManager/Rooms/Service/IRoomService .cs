using MeetingManager.Rooms.Model;

namespace MeetingManager.Rooms.Service
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room> GetRoomByIdAsync(Guid id);
        Task<Room> CreateRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Guid id, Room room);
        Task<bool> DeleteRoomAsync(Guid id);
    }
}