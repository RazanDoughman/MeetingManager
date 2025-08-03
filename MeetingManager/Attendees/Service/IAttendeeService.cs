using MeetingManager.Attendees.Model;

public interface IAttendeeService
{
    Task<List<Attendee>> GetAllAsync();
    Task<Attendee?> GetByIdAsync(Guid id);
    Task<Attendee> CreateAsync(Attendee attendee);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, UpdateAttendeeDto dto);

}
