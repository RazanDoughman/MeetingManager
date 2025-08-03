using System;

namespace MeetingManager.DTOs.RoomFeature
{
    public class CreateRoomFeatureDto
    {
        public Guid RoomId { get; set; }
        public Guid FeatureId { get; set; }
    }
}
