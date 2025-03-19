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

        public EventAreaController( IEventAreaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetEventAreas()
        {
            return Ok(await _repository.GetEventAreasAsync());
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> GetEventAreaById(int id)
        {
            var eventArea = await _repository.GetEventAreaByIdAsync(id);
            if(eventArea == null) return NotFound();

            return Ok(eventArea);
        }

        [HttpGet("event/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetEventAreaByEventId(int id)
        {
            var eventArea = await _repository.GetEventAreaByEventIdAsync(id);

            if (eventArea == null) return NotFound();

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

            return CreatedAtAction(nameof(GetEventAreaById), new { id = newEventArea.EventAreaID }, newEventArea);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<EventArea>> UpdateEventArea(int id, [FromBody] EventAreaDto eventArea)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedEventArea = await _repository.UpdateEventAreaAsync(id, eventArea, User);
            if (updatedEventArea == null) return Forbid();

            return Ok(updatedEventArea);
        }

        [Authorize(Roles = "1,2")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteEventArea(int id)
        {
            var success = await _repository.DeleteEventAreaAsync(id, User);

            if (!success) return Forbid();

            return Ok();
        }
    }
}
