using MeetingManager;
using MeetingManager.Meetings.DTO;
using MeetingManager.Meetings.Model;
using MeetingManager.Service;
using MeetingManager.Users.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MeetingManager.Profile.Controller
{
    [ApiController]
    [Route("api/profile")]
    [Authorize] // any signed-in user
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly MeetingQueryService _mq;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            AppDbContext db,
            MeetingQueryService mq,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _mq = mq;
            _userManager = userManager;
        }

        // GET /api/profile/upcoming?scope=mine|invited&page=1&pageSize=10
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming(
            [FromQuery] string scope = "mine",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            // Map Identity -> domain User (same approach you use in MeetingController.Create)
            var identityId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityId))
                return Unauthorized("No subject/id claim in token.");

            var identityUser = await _userManager.FindByIdAsync(identityId);
            if (identityUser == null || string.IsNullOrWhiteSpace(identityUser.Email))
                return Unauthorized("Identity user not found or has no email.");

            var idEmail = identityUser.Email.Trim().ToLowerInvariant();

            var domainUser = await _db.AppUsers
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == idEmail, ct);

            if (domainUser == null)
                return BadRequest("Signed-in user not found in application Users table.");

            var (items, total) = await _mq.GetUpcomingAsync(domainUser.Id, scope, page, pageSize, ct);
            return Ok(new { items, total, page, pageSize });


        }


        [HttpGet("past")]
        [Authorize] // or your CanView policy
        public async Task<IActionResult> GetPast(
            [FromQuery] string scope = "all",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // map identity -> app user (same pattern you already use)
            var identityId = User.FindFirst("sub")?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityId))
                return Unauthorized("No subject/id claim in token.");

            var identityUser = await _userManager.FindByIdAsync(identityId);
            if (identityUser == null || string.IsNullOrWhiteSpace(identityUser.Email))
                return Unauthorized("Identity user not found or has no email.");

            var idEmail = identityUser.Email.Trim().ToLowerInvariant();
            var currentUser = await _db.AppUsers
                .SingleOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == idEmail);
            if (currentUser == null)
                return BadRequest("Signed-in user not found in application Users table.");

            var now = DateTime.UtcNow;

            // base queries
            var mineQ = _db.Meetings
                .AsNoTracking()
                .Where(m => m.EndTime < now && m.UserId == currentUser.Id);

            var invitedQ = _db.Attendees
                .AsNoTracking()
                .Where(a => a.UserId == currentUser.Id && !a.IsOrganizer)
                .Join(_db.Meetings.AsNoTracking(),
                      a => a.MeetingId, m => m.Id,
                      (a, m) => m)
                .Where(m => m.EndTime < now);

            IQueryable<Meeting> finalQ = scope.ToLowerInvariant() switch
            {
                "mine" => mineQ,
                "invited" => invitedQ,
                _ => mineQ.Union(invitedQ) // all
            };

            // distinct + order + page
            finalQ = finalQ
                .Distinct()
                .OrderByDescending(m => m.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var items = await finalQ
                .Include(m => m.Room)
                .Include(m => m.User)
                .Select(m => new
                {
                    id = m.Id,
                    meetingId = m.Id,    // keep both for frontend compatibility
                    title = m.Title,
                    agenda = m.Agenda,
                    startTime = m.StartTime,
                    endTime = m.EndTime,
                    roomId = m.RoomId,
                    roomName = m.Room.RoomName,
                    organizerId = m.UserId,
                    organizerName = m.User.Name
                })
                .ToListAsync();

            return Ok(new { items });
        }

    }
}
