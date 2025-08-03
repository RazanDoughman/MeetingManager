using System;

namespace MeetingManager.Rooms.Model
{
    public class Room
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
    }
}
