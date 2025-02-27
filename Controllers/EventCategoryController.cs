using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            return await _context.EventCategories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventCategory>> GetEventCategoryById(int id)
        {
            var eventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (eventCategory == null)
            {
                return NotFound();
            }

            return Ok(eventCategory);
        }

        [HttpPost]
        public async Task<ActionResult<EventCategory>> CreateEventCategory([FromBody] EventCategoryDto eventCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            EventCategory newEventCategory = new EventCategory
            {
                EventID = eventCategory.EventID,
                CategoryID = eventCategory.CategoryID,
            };

            _context.EventCategories.Add(newEventCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventCategoryById), new { id = newEventCategory.EventCategoryID }, newEventCategory);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EventCategory>> UpdateEventCategory(int id, [FromBody] EventCategoryDto eventCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (existingEventCategory == null)
            {
                return NotFound();
            }

            existingEventCategory.EventID = eventCategory.EventID;
            existingEventCategory.CategoryID = eventCategory.CategoryID;

            await _context.SaveChangesAsync();

            return Ok(existingEventCategory);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEventCategory(int id)
        {
            var eventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (eventCategory == null)
            {
                return NotFound();
            }

            _context.EventCategories.Remove(eventCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
