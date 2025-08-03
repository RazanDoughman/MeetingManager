using MeetingManager;
using MeetingManager.ActionItems.Model;
using Microsoft.EntityFrameworkCore;

public class ActionItemService : IActionItemService
{
    private readonly AppDbContext _context;

    public ActionItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActionItem>> GetAllAsync()
    {
        return await _context.ActionItems
            .Include(ai => ai.Meeting)
            .Include(ai => ai.CreatedBy)
            .Include(ai => ai.AssignedTo)
            .ToListAsync();
    }

    public async Task<ActionItem?> GetByIdAsync(Guid id)
    {
        return await _context.ActionItems
            .Include(ai => ai.Meeting)
            .Include(ai => ai.CreatedBy)
            .Include(ai => ai.AssignedTo)
            .FirstOrDefaultAsync(ai => ai.Id == id);
    }

    public async Task<ActionItem> CreateAsync(ActionItem actionItem)
    {
        actionItem.Id = Guid.NewGuid();
        actionItem.CreatedAt = DateTime.UtcNow;

        _context.ActionItems.Add(actionItem);
        await _context.SaveChangesAsync();
        return actionItem;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateActionItemDto dto)
    {
        var actionItem = await _context.ActionItems.FindAsync(id);
        if (actionItem == null) return false;

        actionItem.ActionApproval = dto.ActionApproval;
        actionItem.ActionStatus = dto.ActionStatus;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var actionItem = await _context.ActionItems.FindAsync(id);
        if (actionItem == null) return false;

        _context.ActionItems.Remove(actionItem);
        await _context.SaveChangesAsync();
        return true;
    }
}
