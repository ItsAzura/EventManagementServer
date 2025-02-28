using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<EventArea>>> GetEventAreas()
        {
            return await _context.EventAreas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventArea>> GetEventAreaById(int id)
        {
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventAreaID == id);

            if (eventArea == null) return NotFound();

            return Ok(eventArea);
        }

        [HttpGet("event/{id}")]
        public async Task<ActionResult<EventArea>> GetEventAreaByEventId(int id)
        {
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventID == id);

            if (eventArea == null) return NotFound();

            return Ok(eventArea);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventArea>> CreateEventArea([FromBody] EventAreaDto eventArea)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            EventArea newEventArea = new EventArea
            {
                EventID = eventArea.EventID,
                AreaName = eventArea.AreaName,
                Capacity = eventArea.Capacity,
            };

            _context.EventAreas.Add(newEventArea);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventAreaById), new { id = newEventArea.EventAreaID }, newEventArea);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<EventArea>> UpdateEventArea(int id, [FromBody] EventAreaDto eventArea)
        { 
            var existingEventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventAreaID == id);

            if (existingEventArea == null) return NotFound();

            if(!ModelState.IsValid) return BadRequest(ModelState);

            existingEventArea.EventID = eventArea.EventID;
            existingEventArea.AreaName = eventArea.AreaName;
            existingEventArea.Capacity = eventArea.Capacity;

            await _context.SaveChangesAsync();
            return Ok(existingEventArea);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEventArea(int id)
        {
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventAreaID == id);

            if (eventArea == null) return NotFound();

            _context.EventAreas.Remove(eventArea);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
