using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class EventAreaController : Controller
    {
        private readonly EventDbContext _context;

        public EventAreaController(EventDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetEventAreas()
        {
            return await _context.EventAreas.ToListAsync();
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> GetEventAreaById(int id)
        {
            var eventAreas = await _context.EventAreas
                .Where(ea => ea.EventID == id)
                .ToListAsync();

            if (!eventAreas.Any()) return NotFound();

            return Ok(eventAreas);
        }

        [HttpGet("event/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> GetEventAreaByEventId(int id)
        {
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventID == id);

            if (eventArea == null) return NotFound();

            return Ok(eventArea);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> CreateEventArea([FromBody] EventAreaDto eventArea)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            EventArea newEventArea = new EventArea
            {
                EventID = eventArea.EventID,
                AreaName = eventArea.AreaName,
                Capacity = eventArea.Capacity,
            };

            if (newEventArea?.Event == null || (newEventArea.Event.CreatedBy.ToString() != userId && userRole != "1"))
                return Forbid();

            _context.EventAreas.Add(newEventArea);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventAreaById), new { id = newEventArea.EventAreaID }, newEventArea);
        }

        [Authorize]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> UpdateEventArea(int id, [FromBody] EventAreaDto eventArea)
        { 
            var existingEventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventAreaID == id);

            if (existingEventArea == null) return NotFound();

            if(!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existingEventArea?.Event == null || (existingEventArea.Event.CreatedBy.ToString() != userId && userRole != "1"))
                return Forbid();


            existingEventArea.EventID = eventArea.EventID;
            existingEventArea.AreaName = eventArea.AreaName;
            existingEventArea.Capacity = eventArea.Capacity;

            await _context.SaveChangesAsync();
            return Ok(existingEventArea);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteEventArea(int id)
        {
            var eventArea = await _context.EventAreas
                .Include(e => e.Event)
                .FirstOrDefaultAsync(ea => ea.EventAreaID == id);

            if (eventArea == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (eventArea.Event?.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            _context.EventAreas.Remove(eventArea);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
