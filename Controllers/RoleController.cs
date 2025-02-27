using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly EventDbContext _context;

        public RoleController(EventDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRoleById(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null) return NotFound();

            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole([FromBody] RoleDto role)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Role newRole = new Role
            {
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
            };

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoleById), new { id = newRole.RoleID }, newRole);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Role>> UpdateRole(int id, [FromBody] RoleDto role)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (existingRole == null) return NotFound();

            existingRole.RoleName = role.RoleName;
            existingRole.RoleDescription = role.RoleDescription;

            await _context.SaveChangesAsync();

            return Ok(existingRole);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null) return NotFound();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
