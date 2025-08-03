using MeetingManager.ActionItems.Model;

public interface IActionItemService
{
    Task<List<ActionItem>> GetAllAsync();
    Task<ActionItem?> GetByIdAsync(Guid id);
    Task<ActionItem> CreateAsync(ActionItem actionItem);
    Task<bool> UpdateAsync(Guid id, UpdateActionItemDto dto);
    Task<bool> DeleteAsync(Guid id);
}
