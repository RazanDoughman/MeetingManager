using MeetingManager.Notes.Model;

public interface INoteService
{
    Task<List<Note>> GetAllAsync();
    Task<Note?> GetByIdAsync(Guid id);
    Task<Note> CreateAsync(Note note);
    Task<bool> DeleteAsync(Guid id);
}
