using MeetingManager;
using MeetingManager.DTOs.RoomFeature;
using MeetingManager.RoomFeatures.Model;
using MeetingManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingManager.RoomFeatures.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class RoomFeatureController : ControllerBase
    {
        private readonly IRoomFeatureService _service;

        public RoomFeatureController(IRoomFeatureService service)
        {
            _service = service;
        }

        // GET: api/RoomFeature
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomFeatureDto>>> GetAll()
        {
            var roomFeatures = await _service.GetAllAsync();

            var result = roomFeatures.Select(rf => new RoomFeatureDto
            {
                Id = rf.Id,
                RoomId = rf.RoomId,
                FeatureId = rf.FeatureId,
                RoomName = rf.Room?.RoomName,
                FeatureName = rf.Feature?.FeatureName
            });

            return Ok(result);
        }



        // POST: api/RoomFeature
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<RoomFeatureDto>> Create(CreateRoomFeatureDto dto)
        {
            var newEntity = new RoomFeature
            {
                RoomId = dto.RoomId,
                FeatureId = dto.FeatureId
            };

            var created = await _service.CreateAsync(newEntity);

            var result = new RoomFeatureDto
            {
                Id = created.Id,
                RoomId = created.RoomId,
                FeatureId = created.FeatureId,
                RoomName = created.Room?.RoomName,
                FeatureName = created.Feature?.FeatureName
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

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