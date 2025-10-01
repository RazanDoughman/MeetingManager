using MeetingManager;
using MeetingManager.Rooms.DTO;
using MeetingManager.Rooms.Model;
using MeetingManager.Rooms.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingManager.Rooms.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(Guid id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound();
            return Ok(room);
        }

        // PUT: api/Rooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(Guid id, Room room)
        {
            if (id != room.Id)
                return BadRequest();

            var updated = await _roomService.UpdateRoomAsync(id, room);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // POST: api/Rooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Room>> PostRoom(Room room)
        {
            var created = await _roomService.CreateRoomAsync(room);
            return CreatedAtAction(nameof(GetRoom), new { id = created.Id }, created);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var deleted = await _roomService.DeleteRoomAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id:guid}/availability")]
        public async Task<ActionResult<DailyAvailabilityDto>> GetAvailability(
          Guid id,
          [FromQuery] DateOnly date,     // requires .NET 7/8 DateOnly binding
          [FromQuery] int slotMinutes = 30,
          [FromQuery(Name = "startHour")] int startHourLocal = 9,
          [FromQuery(Name = "endHour")] int endHourLocal = 17)
        {
            var result = await _roomService.GetDailyAvailabilityAsync(id, date, slotMinutes, startHourLocal, endHourLocal);
            if (result is null) return NotFound("Room not found.");
            return Ok(result);
        }
    }
}