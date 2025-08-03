using MeetingManager.Meetings.Model;

public interface IMeetingService
{
    Task<List<Meeting>> GetAllAsync();
    Task<Meeting?> GetByIdAsync(Guid id);
    Task<Meeting> CreateAsync(Meeting meeting);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, UpdateMeetingDto dto);

}
