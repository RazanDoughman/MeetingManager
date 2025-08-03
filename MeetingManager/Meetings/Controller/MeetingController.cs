using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetingManager;
using MeetingManager.Meetings.Model;

namespace MeetingManager.Meetings.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _service;

        public MeetingController(IMeetingService service)
        {
            _service = service;
        }


        // GET: api/Meeting
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingDto>>> GetAll()
        {
            var meetings = await _service.GetAllAsync();

            var result = meetings.Select(m => new MeetingDto
            {
                Id = m.Id,
                Title = m.Title,
                Agenda = m.Agenda,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                Status = m.Status,
                RoomId = m.RoomId,
                RoomName = m.Room?.RoomName,
                UserId = m.UserId,
                UserName = m.User?.Name
            });

            return Ok(result);
        }

        // PUT: api/Meeting/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateMeetingDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }


        // POST: api/Meeting
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MeetingDto>> Create(CreateMeetingDto dto)
        {
            var meeting = new Meeting
            {
                RoomId = dto.RoomId,
                UserId = dto.UserId,
                Title = dto.Title,
                Agenda = dto.Agenda,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = dto.Status
            };

            var created = await _service.CreateAsync(meeting);

            var result = new MeetingDto
            {
                Id = created.Id,
                Title = created.Title,
                Agenda = created.Agenda,
                StartTime = created.StartTime,
                EndTime = created.EndTime,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                Status = created.Status,
                RoomId = created.RoomId,
                UserId = created.UserId
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}