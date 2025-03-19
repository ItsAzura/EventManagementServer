using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using EventManagementServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class EventCategoryController : Controller
    {
        private readonly EventDbContext _context;
        private readonly IEventCategoryRepository _repository;

        public EventCategoryController(EventDbContext context, IEventCategoryRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        [Authorize]
        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            return Ok(await _repository.GetEventCategoriesAsync());
        }

        [Authorize]
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventCategory>> GetEventCategoryById(int id)
        {
            var eventCategory = await _repository.GetEventCategoryByIdAsync(id);

            if (eventCategory == null) return NotFound();

            return Ok(eventCategory);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventCategory>> CreateEventCategory([FromBody] EventCategoryDto eventCategory)
        { 
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newEventCategory = await _repository.CreateEventCategoryAsync(eventCategory, User);

            if (newEventCategory == null) return Forbid();

            return CreatedAtAction(nameof(GetEventCategoryById), new { id = newEventCategory.EventCategoryID }, newEventCategory);
        }

        [Authorize]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventCategory>> UpdateEventCategory(int id, [FromBody] EventCategoryDto eventCategory)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedEventCategory = await _repository.UpdateEventCategoryAsync(id, eventCategory, User);

            if (updatedEventCategory == null) return NotFound();

            return Ok(updatedEventCategory);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteEventCategory(int id)
        {
            var success = await _repository.DeleteEventCategoryAsync(id, User);

            if (!success) return Forbid();

            return NoContent();
        }
    }

}
