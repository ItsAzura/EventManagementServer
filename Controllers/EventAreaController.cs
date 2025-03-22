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
    public class EventAreaController : Controller
    {
        private readonly IEventAreaRepository _repository;
        private readonly ILogger<EventAreaController> _logger;

        public EventAreaController( IEventAreaRepository repository, ILogger<EventAreaController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetEventAreas()
        {
            var respone = await _repository.GetEventAreasAsync();

            _logger.LogInformation($"Get all event areas: {respone}");

            return Ok(respone);
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> GetEventAreaById(int id)
        {
            var eventArea = await _repository.GetEventAreaByIdAsync(id);
            if(eventArea == null) return NotFound();

            _logger.LogInformation($"Get event area by id: {eventArea}");

            return Ok(eventArea);
        }

        [HttpGet("event/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetEventAreaByEventId(int id)
        {
            var eventArea = await _repository.GetEventAreaByEventIdAsync(id);

            if (eventArea == null) return NotFound();

            _logger.LogInformation($"Get event area by event id: {eventArea}");

            return Ok(eventArea);
        }

        [Authorize(Roles = "1,2")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> CreateEventArea([FromBody] EventAreaDto eventArea)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var newEventArea = await _repository.CreateEventAreaAsync(eventArea, User);
            if (newEventArea == null) return Forbid();

            _logger.LogInformation($"Create event area: {newEventArea}");

            return CreatedAtAction(nameof(GetEventAreaById), new { id = newEventArea.EventAreaID }, newEventArea);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> UpdateEventArea(int id, [FromBody] EventAreaDto eventArea)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedEventArea = await _repository.UpdateEventAreaAsync(id, eventArea, User);
            if (updatedEventArea == null) return Forbid();

            _logger.LogInformation($"Update event area: {updatedEventArea}");

            return Ok(updatedEventArea);
        }

        [Authorize(Roles = "1,2")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteEventArea(int id)
        {
            var success = await _repository.DeleteEventAreaAsync(id, User);

            if (!success) return Forbid();

            _logger.LogInformation($"Delete event area: {id}");

            return Ok();
        }
    }
}
