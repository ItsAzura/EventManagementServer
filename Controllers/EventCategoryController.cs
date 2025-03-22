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
        private readonly ILogger<EventCategoryController> _logger;

        public EventCategoryController(EventDbContext context, IEventCategoryRepository repository, ILogger<EventCategoryController> logger)
        {
            _context = context;
            _repository = repository;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            var response = await _repository.GetEventCategoriesAsync();

            _logger.LogInformation($"Get all event categories: {response}");

            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventCategory>> GetEventCategoryById(int id)
        {
            var eventCategory = await _repository.GetEventCategoryByIdAsync(id);

            if (eventCategory == null) return NotFound();

            _logger.LogInformation($"Get event category by id: {eventCategory}");

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

            _logger.LogInformation($"New event category: {newEventCategory}");

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

            _logger.LogInformation($"Update event category: {updatedEventCategory}");

            return Ok(updatedEventCategory);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteEventCategory(int id)
        {
            var success = await _repository.DeleteEventCategoryAsync(id, User);

            if (!success) return Forbid();

            _logger.LogInformation($"Delete event category: {id}");

            return NoContent();
        }
    }

}
