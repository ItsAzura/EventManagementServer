using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Role")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoleController : Controller
    {
        private readonly EventDbContext _context;
        private readonly ILogger<RoleController> _logger;

        public RoleController(EventDbContext context, ILogger<RoleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();

            if (roles == null) return NotFound();

            _logger.LogInformation($"Get all roles: {roles}");

            return Ok(roles);
        }

        [Authorize(Roles = "1")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Role>> GetRoleById(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null) return NotFound();

            _logger.LogInformation($"Get role by id: {role}");

            return Ok(role);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Role>> CreateRole([FromBody] RoleDto role)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Role newRole = new Role
            {
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
            };

            _logger.LogInformation($"Create new role: {newRole}");

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoleById), new { id = newRole.RoleID }, newRole);
        }

        [Authorize(Roles = "1")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Role>> UpdateRole(int id, [FromBody] RoleDto role)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (existingRole == null) return NotFound();

            existingRole.RoleName = role.RoleName;
            existingRole.RoleDescription = role.RoleDescription;

            _logger.LogInformation($"Update role: {existingRole}");

            await _context.SaveChangesAsync();

            return Ok(existingRole);
        }

        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null) return NotFound();

            _logger.LogInformation($"Delete role: {role}");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
