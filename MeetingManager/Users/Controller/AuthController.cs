using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MeetingManager.Users.DTO;     // your LoginRequest, RegisterRequest
using MeetingManager.Users.Model;   // ApplicationUser
using Microsoft.EntityFrameworkCore;

namespace MeetingManager.Users.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public AuthController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm, IConfiguration cfg, AppDbContext db)
        { _userManager = um; _signInManager = sm; _config = cfg; _db = db; }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _userManager.FindByNameAsync(req.Username)
                       ?? await _userManager.FindByEmailAsync(req.Username);
            if (user == null) return Unauthorized(new { message = "Invalid credentials" });

            var ok = await _signInManager.CheckPasswordSignInAsync(user, req.Password, false);
            if (!ok.Succeeded) return Unauthorized(new { message = "Invalid credentials" });

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                token = GenerateJwt(user, roles),
                user = new { user.Id, user.UserName, user.Email, role = roles.FirstOrDefault() ?? "Guest" }
            });
        }

        // Admin-only user creation (registration)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
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

            // --- Resolve domain role (dbo.Role). Default to "Employee"
            var roleName = string.IsNullOrWhiteSpace(req.Role) ? "Employee" : req.Role;

            var domainRole = await _db.Roles.SingleOrDefaultAsync(r => r.Title == roleName);
            if (domainRole == null)
            {
                domainRole = new MeetingManager.Roles.Model.Role
                {
                    Id = Guid.NewGuid(),
                    Title = roleName
                };
                _db.Roles.Add(domainRole);
                await _db.SaveChangesAsync();
            }

            // --- Ensure domain user (dbo.User) exists for invitations/attendees
            if (!string.IsNullOrWhiteSpace(u.Email))
            {
                var emailNorm = u.Email.Trim().ToLowerInvariant();
                var exists = await _db.AppUsers
                    .AnyAsync(x => x.Email != null && x.Email.Trim().ToLower() == emailNorm);

                if (!exists)
                {
                    var displayName = string.IsNullOrWhiteSpace(u.FullName)
                        ? (string.IsNullOrWhiteSpace(u.UserName) ? u.Email : u.UserName)
                        : u.FullName;

                    _db.AppUsers.Add(new MeetingManager.Users.Model.User
                    {
                        Id = Guid.NewGuid(),
                        Name = displayName,
                        Email = u.Email,
                        CreatedAt = DateTime.UtcNow,
                        RoleId = domainRole.Id   // non-nullable Guid
                    });

                    await _db.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Registered" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var u = await _userManager.FindByIdAsync(id);
            var roles = await _userManager.GetRolesAsync(u!);
            return Ok(new { u!.Id, u.UserName, u.Email, u.FullName, role = roles.FirstOrDefault() ?? "Guest" });
        }

        private string GenerateJwt(ApplicationUser user, IList<string> roles)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? "")
            };
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"], audience: jwt["Audience"],
                claims: claims, expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!)),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
