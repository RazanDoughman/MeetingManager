using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetingManager;
using MeetingManager.Attachments.Model;

namespace MeetingManager.Attachments.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _service;

        public AttachmentController(IAttachmentService service)
        {
            _service = service;
        }

        // GET: api/Attachment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetAll()
        {
            var attachments = await _service.GetAllAsync();
            var result = attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                MeetingId = a.MeetingId,
                MeetingTitle = a.Meeting?.Title,
                FileName = a.FileName,
                Link = a.Link
            });

            return Ok(result);
        }

        // POST: api/Attachment
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AttachmentDto>> Create(CreateAttachmentDto dto)
        {
            var attachment = new Attachment
            {
                MeetingId = dto.MeetingId,
                FileName = dto.FileName,
                Link = dto.Link
            };

            var created = await _service.CreateAsync(attachment);

            var result = new AttachmentDto
            {
                Id = created.Id,
                MeetingId = created.MeetingId,
                FileName = created.FileName,
                Link = created.Link
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }


        // DELETE: api/Attachment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
