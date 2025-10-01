namespace MeetingManager.Rooms.DTO
{
    public sealed class AvailabilitySlotDto
    {
        public DateTime StartLocal { get; set; }  // local time (Asia/Beirut)
        public DateTime EndLocal { get; set; }
        public bool IsBooked { get; set; }
        public string? MeetingTitle { get; set; }
        public Guid? MeetingId { get; set; }
    }

    public sealed class DailyAvailabilityDto
    {
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }   // local date requested
        public int SlotMinutes { get; set; }
        public int StartHourLocal { get; set; }  // e.g., 9
        public int EndHourLocal { get; set; }  // e.g., 17
        public List<AvailabilitySlotDto> Slots { get; set; } = new();
    }
}
