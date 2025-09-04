using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingManager.Roles.Model;              
using Microsoft.AspNetCore.Authorization;      
using Microsoft.AspNetCore.Identity;           
using Microsoft.AspNetCore.Mvc;

namespace MeetingManager.Roles.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roles;

        public RoleController(RoleManager<IdentityRole> roles)
        {
            _roles = roles;
        }

        // GET: api/Role
        [HttpGet]
        [Authorize(Policy = "CanView")]
        public ActionResult<IEnumerable<Role>> GetRole()
        {
            
            var list = _roles.Roles
                .Select(r => new Role
                {
                    Id = TryGuid(r.Id),       
                    Title = r.Name ?? string.Empty
                })
                .ToList();

            return Ok(list);
        }

        // GET: api/Role/{id}
        // (Identity role IDs are strings; keep the route param as string)
        [HttpGet("{id}")]
        [Authorize(Policy = "CanView")]
        public async Task<ActionResult<Role>> GetRole(string id)
        {
            var r = await _roles.FindByIdAsync(id);
            if (r is null) return NotFound();

            return new Role
            {
                Id = TryGuid(r.Id),
                Title = r.Name ?? string.Empty
            };
        }

        // PUT: api/Role/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, Role role)
        {
            var r = await _roles.FindByIdAsync(id);
            if (r is null) return NotFound();

            if (role is null || string.IsNullOrWhiteSpace(role.Title))
                return BadRequest("Role title is required.");

            r.Name = role.Title;
            r.NormalizedName = role.Title.ToUpperInvariant();

            var update = await _roles.UpdateAsync(r);
            if (!update.Succeeded) return BadRequest(update.Errors);

            return NoContent();
        }


        // POST: api/Role
        // Accepts your Role model; we use Title as the role name
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            if (role is null || string.IsNullOrWhiteSpace(role.Title))
                return BadRequest("Role title is required.");

            if (await _roles.RoleExistsAsync(role.Title))
                return Conflict(new { message = "Role already exists." });

            var create = await _roles.CreateAsync(new IdentityRole(role.Title));
            if (!create.Succeeded) return BadRequest(create.Errors);

            var created = await _roles.FindByNameAsync(role.Title);
            var dto = new Role
            {
                Id = TryGuid(created!.Id),
                Title = created.Name!
            };

            // Note: created.Id is string; we pass it as route value
            return CreatedAtAction(nameof(GetRole), new { id = created.Id }, dto);
        }

        // DELETE: api/Role/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var r = await _roles.FindByIdAsync(id);
            if (r is null) return NotFound();

            var del = await _roles.DeleteAsync(r);
            if (!del.Succeeded) return BadRequest(del.Errors);

            return NoContent();
        }

        private static Guid TryGuid(string s) =>
            Guid.TryParse(s, out var g) ? g : Guid.Empty;
    }
}