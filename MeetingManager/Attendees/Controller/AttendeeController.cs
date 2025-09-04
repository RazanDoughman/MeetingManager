using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MeetingManager;
using MeetingManager.Attendees.Model;

namespace MeetingManager.Attendees.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class AttendeeController : ControllerBase
    {
        private readonly IAttendeeService _service;

        public AttendeeController(IAttendeeService service)
        {
            _service = service;
        }

        // GET: api/Attendee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendeeDto>>> GetAll()
        {
            var attendees = await _service.GetAllAsync();
            var result = attendees.Select(a => new AttendeeDto
            {
                Id = a.Id,
                MeetingId = a.MeetingId,
                MeetingTitle = a.Meeting?.Title,
                UserId = a.UserId,
                UserName = a.User?.Name,
                Attended = a.Attended
            });
            return Ok(result);
        }



        // PUT: api/Attendee/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "CanBook")]
        public async Task<IActionResult> Update(Guid id, UpdateAttendeeDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }


        // POST: api/Attendee
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "CanBook")]
        public async Task<ActionResult<AttendeeDto>> Create(CreateAttendeeDto dto)
        {
            var attendee = new Attendee
            {
                MeetingId = dto.MeetingId,
                UserId = dto.UserId,
                Attended = dto.Attended
            };

            var created = await _service.CreateAsync(attendee);

            var result = new AttendeeDto
            {
                Id = created.Id,
                MeetingId = created.MeetingId,
                UserId = created.UserId,
                Attended = created.Attended
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        // DELETE: api/Attendee/5
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
