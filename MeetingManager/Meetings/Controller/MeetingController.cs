using MeetingManager;
using MeetingManager.Attendees.Model;
using MeetingManager.Meetings.DTO;
using MeetingManager.Meetings.Model;
using MeetingManager.Users.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace MeetingManager.Meetings.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanView")]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _service;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeetingController(IMeetingService service, AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _db = db;
            _userManager = userManager;
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(Guid id, UpdateMeetingDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }


        [HttpPost]
        [Authorize(Policy = "CanBook")]
        public async Task<ActionResult<MeetingDto>> Create([FromBody] CreateMeetingDto dto)
        {

            var identityId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityId))
                return Unauthorized("No subject/id claim in token.");

            var identityUser = await _userManager.FindByIdAsync(identityId);
            if (identityUser == null)
                return Unauthorized("Identity user not found.");

            if (string.IsNullOrWhiteSpace(identityUser.Email))
                return BadRequest("Identity user has no email; cannot link to application user.");


            var idEmail = identityUser.Email.Trim().ToLowerInvariant();

            var domainUser = await _db.AppUsers
                .SingleOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == idEmail);

            if (domainUser == null)
                return BadRequest(new
                {
                    error = "Signed-in user not found in application Users table (matched by email).",
                    identity = new { identityUser.Id, identityUser.UserName, identityUser.Email }
                });


            var roomExists = await _db.Rooms.AnyAsync(r => r.Id == dto.RoomId);
            if (!roomExists) return BadRequest("Room not found.");

            if (dto.EndTime <= dto.StartTime)
                return BadRequest("End time must be after start time.");

            var startUtc = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Utc);

            var meeting = new Meeting
            {
                RoomId = dto.RoomId,
                UserId = domainUser.Id,
                Title = dto.Title,
                Agenda = dto.Agenda,
                StartTime = startUtc,
                EndTime = endUtc,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status
            };
            try {
                var created = await _service.CreateAsync(meeting);

                // Add creator as organizer attendee
                _db.Attendees.Add(new Attendee
                {
                    Id = Guid.NewGuid(),
                    MeetingId = created.Id,
                    UserId = domainUser.Id,
                    IsOrganizer = true,
                    Attended = false 
                });

                if (dto.AttendeeUserIds != null && dto.AttendeeUserIds.Count > 0)
                {
                    foreach (var uid in dto.AttendeeUserIds.Distinct().Where(id => id != domainUser.Id))
                    {
                        _db.Attendees.Add(new Attendee
                        {
                            Id = Guid.NewGuid(),
                            MeetingId = created.Id,
                            UserId = uid,
                            IsOrganizer = false,
                            Attended = false
                        });
                    }
                }

                await _db.SaveChangesAsync();

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
                    RoomName = created.Room?.RoomName,
                    UserId = created.UserId,
                    UserName = created.User?.Name
                };

                return Created($"/api/Meeting/{result.Id}", result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already booked"))
            {
                return Conflict("Room is already booked for the selected time.");
            }
        }

        [HttpGet("picker-users")]
        public async Task<ActionResult<IEnumerable<object>>> GetPickerUsers()
        {
            var users = await _db.AppUsers
                .Select(u => new { id = u.Id, name = u.Name, email = u.Email })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]             
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }


        public class InviteUsersDto
        {
            public List<Guid> UserIds { get; set; } = new();
        }

        [HttpPost("{id:guid}/invite")]
        [Authorize(Policy = "CanBook")] 
        public async Task<IActionResult> Invite(Guid id, [FromBody] InviteUsersDto dto)
        {
            var identityId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityId))
                return Unauthorized("No subject/id claim in token.");

            var identityUser = await _userManager.FindByIdAsync(identityId);
            if (identityUser == null || string.IsNullOrWhiteSpace(identityUser.Email))
                return Unauthorized("Identity user not found or has no email.");

            var idEmail = identityUser.Email.Trim().ToLowerInvariant();
            var domainUser = await _db.AppUsers
                .SingleOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == idEmail);
            if (domainUser == null)
                return BadRequest("Signed-in user not found in application Users table.");

            var meeting = await _db.Meetings.FirstOrDefaultAsync(m => m.Id == id);
            if (meeting == null) return NotFound("Meeting not found.");

            // Organizer-only guard
            if (meeting.UserId != domainUser.Id)
                return Forbid();

            if (dto.UserIds == null || dto.UserIds.Count == 0)
                return Ok(new { added = 0 });

            var targetIds = dto.UserIds
                .Where(uid => uid != domainUser.Id) // don’t re-add organizer
                .Distinct()
                .ToList();

            if (targetIds.Count == 0) return Ok(new { added = 0 });

            // Fetch already invited users for this meeting
            var existing = await _db.Attendees
                .Where(a => a.MeetingId == id && targetIds.Contains(a.UserId))
                .Select(a => a.UserId)
                .ToListAsync();

            var toAdd = targetIds
                .Except(existing)
                .Select(uid => new Attendee
                {
                    Id = Guid.NewGuid(),
                    MeetingId = id,
                    UserId = uid,
                    IsOrganizer = false,
                    Attended = false
                })
                .ToList();

            if (toAdd.Count == 0) return Ok(new { added = 0 });

            _db.Attendees.AddRange(toAdd);
            await _db.SaveChangesAsync();

            return Ok(new { added = toAdd.Count });
        }

        [HttpPatch("{id:guid}/reschedule")]
        public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleMeetingDto dto)
        {
            var identityId = User.FindFirst("sub")?.Value
                  ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityId))
                return Unauthorized("No subject/id claim in token.");

            var identityUser = await _userManager.FindByIdAsync(identityId);
            if (identityUser == null || string.IsNullOrWhiteSpace(identityUser.Email))
                return Unauthorized("Identity user not found or has no email.");

            var idEmail = identityUser.Email.Trim().ToLowerInvariant();
            var domainUser = await _db.AppUsers
                .SingleOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == idEmail);
            if (domainUser == null)
                return BadRequest("Signed-in user not found in application Users table.");

            // 2) Load meeting and authorize: organizer or admin
            var meeting = await _db.Meetings.FirstOrDefaultAsync(m => m.Id == id);
            if (meeting == null) return NotFound("Meeting not found.");

            var isOrganizer = meeting.UserId == domainUser.Id;  // Meeting.UserId is Guid
            var isAdmin = User.IsInRole("Admin");

            if (!isOrganizer && !isAdmin)
                return Forbid("You are not allowed to reschedule this meeting.");

            // 3) Proceed with business logic
            try
            {
                var updated = await _service.RescheduleAsync(id, dto.StartTime, dto.EndTime);
                if (updated is null) return NotFound("Meeting not found.");

                return Ok(new
                {
                    updated.Id,
                    updated.Title,
                    updated.RoomId,
                    updated.StartTime,
                    updated.EndTime,
                    updated.Status
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) // overlap/conflict
            {
                return Conflict(ex.Message);
            }
        }


        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var identityId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityId))
                return Unauthorized();

            var identityUser = await _userManager.FindByIdAsync(identityId);
            if (identityUser == null || string.IsNullOrWhiteSpace(identityUser.Email))
                return Unauthorized("Identity user not found or has no email.");

            var idEmail = identityUser.Email.Trim().ToLowerInvariant();
            var domainUser = await _db.AppUsers
                .SingleOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == idEmail);
            if (domainUser == null)
                return BadRequest("Signed-in user not found in application Users table.");

            var isAdmin = User.IsInRole("Admin");

            try
            {
                var success = await _service.CancelAsync(id, domainUser.Id, isAdmin);
                if (!success) return NotFound("Meeting not found or already canceled.");
                return Ok(new { canceled = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("canceled")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MeetingDto>>> GetCanceled()
        {
            var meetings = await _service.GetAllAsync();

            var canceled = meetings
                .Where(m => m.Status == "Canceled")
                .Select(m => new MeetingDto
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

            return Ok(canceled);
        }

        // GET: api/Meeting/calendar
        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendarEvents(
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? organizerId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            var query = _db.Meetings
                .Include(m => m.Room)
                .Include(m => m.User)
                .Where(m => m.Status != "Canceled");

            // Apply filters
            if (roomId.HasValue)
                query = query.Where(m => m.RoomId == roomId.Value);

            if (organizerId.HasValue)
                query = query.Where(m => m.UserId == organizerId.Value);

            if (start.HasValue && end.HasValue)
                query = query.Where(m => m.StartTime >= start.Value && m.EndTime <= end.Value);

            var events = await query
                .Select(m => new
                {
                    id = m.Id,
                    title = m.Title + " (" + (m.Room.RoomName ?? "Room") + ")",
                    start = m.StartTime,
                    end = m.EndTime,
                    roomName = m.Room.RoomName,
                    organizer = m.User.Name
                })
                .ToListAsync();

            return Ok(events);
        }

    }
}