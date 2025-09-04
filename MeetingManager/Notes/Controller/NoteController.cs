using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MeetingManager;
using MeetingManager.Notes.Model;

namespace MeetingManager.Notes.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _service;

        public NoteController(INoteService service)
        {
            _service = service;
        }

        // GET: api/Note
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetAll()
        {
            var notes = await _service.GetAllAsync();

            var result = notes.Select(n => new NoteDto
            {
                Id = n.Id,
                MeetingId = n.MeetingId,
                CreatedByUserId = n.CreatedByUserId,
                NoteBody = n.NoteBody,
                CreatedAt = n.CreatedAt,
                CreatedByUserName = n.CreatedByUser?.Name
            });

            return Ok(result);
        }


        // POST: api/Note
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "CanBook")]
        public async Task<ActionResult<NoteDto>> Create(CreateNoteDto dto)
        {
            var note = new Note
            {
                MeetingId = dto.MeetingId,
                CreatedByUserId = dto.CreatedByUserId,
                NoteBody = dto.NoteBody
            };

            var created = await _service.CreateAsync(note);

            var result = new NoteDto
            {
                Id = created.Id,
                MeetingId = created.MeetingId,
                CreatedByUserId = created.CreatedByUserId,
                NoteBody = created.NoteBody,
                CreatedAt = created.CreatedAt
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        // DELETE: api/Note/5
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