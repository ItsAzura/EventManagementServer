using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class EventCategoryController : Controller
    {
        private readonly EventDbContext _context;

        public EventCategoryController(EventDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            return await _context.EventCategories.ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<EventCategory>> GetEventCategoryById(int id)
        {
            var eventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (eventCategory == null) return NotFound();

            return Ok(eventCategory);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventCategory>> CreateEventCategory([FromBody] EventCategoryDto eventCategory)
        { 
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            EventCategory newEventCategory = new EventCategory
            {
                EventID = eventCategory.EventID,
                CategoryID = eventCategory.CategoryID,
            };

            if (newEventCategory.Event == null || (newEventCategory.Event.CreatedBy.ToString() != userId && userRole != "1"))
                return Forbid();

            _context.EventCategories.Add(newEventCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventCategoryById), new { id = newEventCategory.EventCategoryID }, newEventCategory);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<EventCategory>> UpdateEventCategory(int id, [FromBody] EventCategoryDto eventCategory)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingEventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (existingEventCategory == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existingEventCategory.Event == null || (existingEventCategory.Event.CreatedBy.ToString() != userId && userRole != "1"))
                return Forbid();

            existingEventCategory.EventID = eventCategory.EventID;
            existingEventCategory.CategoryID = eventCategory.CategoryID;

            await _context.SaveChangesAsync();

            return Ok(existingEventCategory);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEventCategory(int id)
        {
            var eventCategory = await _context.EventCategories
                .Include(ec => ec.Event)
                .FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (eventCategory == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (eventCategory.Event?.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            _context.EventCategories.Remove(eventCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
