using MeetingManager.Users.DTO;
using MeetingManager.Users.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingManager.Users.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Admin-only management
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult List()
        {
            var users = _userManager.Users
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    Email = u.Email!,
                    FullName = u.FullName
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);
            return Ok(new UserDetailDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                FullName = u.FullName,
                Roles = roles.ToList()
            });
        }

        // POST: api/users
        // Create a user and assign a single role (Admin/Employee/Guest)
        [HttpPost]
        public async Task<IActionResult> Create(RegisterRequest req)
        {
            // Ensure role exists
            if (!string.IsNullOrWhiteSpace(req.Role) && !await _roleManager.RoleExistsAsync(req.Role))
                return BadRequest(new { message = $"Role '{req.Role}' does not exist." });

            var u = new ApplicationUser
            {
                UserName = req.Username,
                Email = req.Email,
                EmailConfirmed = true,
                FullName = req.FullName
            };

            var result = await _userManager.CreateAsync(u, req.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!string.IsNullOrWhiteSpace(req.Role))
                await _userManager.AddToRoleAsync(u, req.Role);

            return CreatedAtAction(nameof(GetById), new { id = u.Id }, new { u.Id });
        }

        // PUT: api/users/{id}
        // Update basic fields and (optionally) replace role with a new single role
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UpdateUserRequest req)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(req.Email)) u.Email = req.Email;
            if (!string.IsNullOrWhiteSpace(req.UserName)) u.UserName = req.UserName;
            if (req.FullNameSet) u.FullName = req.FullName; // allows null to be set

            var res = await _userManager.UpdateAsync(u);
            if (!res.Succeeded) return BadRequest(res.Errors);

            // Handle role change if provided
            if (!string.IsNullOrWhiteSpace(req.Role))
            {
                if (!await _roleManager.RoleExistsAsync(req.Role))
                    return BadRequest(new { message = $"Role '{req.Role}' does not exist." });

                var current = await _userManager.GetRolesAsync(u);
                if (current.Any())
                    await _userManager.RemoveFromRolesAsync(u, current);

                await _userManager.AddToRoleAsync(u, req.Role);
            }

            return NoContent();
        }

        // PUT: api/users/{id}/password
        // Admin resets password (no old password required)
        [HttpPut("{id}/password")]
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordRequest req)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(u);
            var result = await _userManager.ResetPasswordAsync(u, token, req.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();

            var res = await _userManager.DeleteAsync(u);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return NoContent();
        }
    }
}
