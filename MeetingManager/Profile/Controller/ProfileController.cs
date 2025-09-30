using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MeetingManager.Users.Model;
using MeetingManager.Meetings.DTO;
using MeetingManager.Service;
using MeetingManager;

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
    }
}
