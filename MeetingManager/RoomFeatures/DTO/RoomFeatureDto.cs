using System;

namespace MeetingManager.DTOs.RoomFeature
{
    public class RoomFeatureDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid FeatureId { get; set; }
        public string RoomName { get; set; }
        public string FeatureName { get; set; }
    }
}
