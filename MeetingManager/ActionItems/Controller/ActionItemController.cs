using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MeetingManager;
using MeetingManager.ActionItems.Model;

namespace MeetingManager.ActionItems.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class ActionItemController : ControllerBase
    {
        private readonly IActionItemService _service;

        public ActionItemController(IActionItemService service)
        {
            _service = service;
        }

        // GET: api/ActionItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActionItemDto>>> GetAll()
        {
            var actionItems = await _service.GetAllAsync();
            var result = actionItems.Select(ai => new ActionItemDto
            {
                Id = ai.Id,
                MeetingId = ai.MeetingId,
                MeetingTitle = ai.Meeting?.Title,
                Description = ai.Description,
                ActionApproval = ai.ActionApproval,
                CreatedById = ai.CreatedById,
                CreatedByName = ai.CreatedBy?.Name,
                AssignedToId = ai.AssignedToId,
                AssignedToName = ai.AssignedTo?.Name,
                ActionDeadline = ai.ActionDeadline,
                ActionStatus = ai.ActionStatus,
                CreatedAt = ai.CreatedAt
            });

            return Ok(result);
        }

        // PUT: api/ActionItem/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "CanBook")]
        public async Task<IActionResult> Update(Guid id, UpdateActionItemDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        // POST: api/ActionItem
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "CanBook")]
        public async Task<ActionResult<ActionItemDto>> Create(CreateActionItemDto dto)
        {
            var actionItem = new ActionItem
            {
                MeetingId = dto.MeetingId,
                Description = dto.Description,
                ActionApproval = dto.ActionApproval,
                CreatedById = dto.CreatedById,
                AssignedToId = dto.AssignedToId,
                ActionDeadline = dto.ActionDeadline,
                ActionStatus = dto.ActionStatus
            };

            var created = await _service.CreateAsync(actionItem);

            var result = new ActionItemDto
            {
                Id = created.Id,
                MeetingId = created.MeetingId,
                Description = created.Description,
                ActionApproval = created.ActionApproval,
                CreatedById = created.CreatedById,
                AssignedToId = created.AssignedToId,
                ActionDeadline = created.ActionDeadline,
                ActionStatus = created.ActionStatus,
                CreatedAt = created.CreatedAt
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        // DELETE: api/ActionItem/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
