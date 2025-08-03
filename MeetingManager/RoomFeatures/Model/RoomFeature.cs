using MeetingManager.Features.Model;
using MeetingManager.Rooms.Model;
using System;

namespace MeetingManager.RoomFeatures.Model
{
    public class RoomFeature
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid FeatureId { get; set; }

        public Room Room { get; set; }
        public Feature Feature { get; set; }
    }
}
