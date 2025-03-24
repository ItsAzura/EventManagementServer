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
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Ticket")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TicketController : Controller
    {
        private readonly EventDbContext _context;
        private readonly ILogger<TicketController> _logger;

        public TicketController(EventDbContext context, ILogger<TicketController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets(int page = 1, int pageSize = 10, int? Quantity = null, decimal? Price = null, string? search = null)
        {
            if(page < 1 || pageSize < 1) return BadRequest("Invalid page or pageSize");

            var query = _context.Tickets.AsQueryable();

            if(Quantity.HasValue)
            {
                query = query.Where(t => t.Quantity == Quantity.Value);
            }

            if(Price.HasValue)
            {
                query = query.Where(t => t.Price == Price.Value);
            }

            if(!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TicketName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderBy(t => t.TicketID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if(tickets == null) return NotFound();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Tickets = tickets
            };

            _logger.LogInformation($"Get tickets: {response}");

            return Ok(response);
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ticket>> GetTicketById(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            _logger.LogInformation($"Get ticket by id: {ticket}");

            return Ok(ticket);
        }

        [HttpGet("eventarea/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetTicketByEventAreaId(int id)
        {
            var ticket = await _context.Tickets
                .Where(t => t.EventAreaID == id)
                .ToListAsync();

            if (ticket == null) return NotFound();

            _logger.LogInformation($"Get ticket by event area id: {ticket}");

            return Ok(ticket);
        }

        [Authorize(Roles = "1,2")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Ticket>> CreateTicket([FromBody] TicketDto ticket)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByeventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);

            if (eventArea == null)
            {
                ModelState.AddModelError("EventAreaID", "Event Area not found");
                return BadRequest(ModelState);
            }

            var totalExistingQuantity = await _context.Tickets
                .Where(t => t.EventAreaID == ticket.EventAreaID)
                .SumAsync(t =>(int?) t.Quantity) ?? 0;

            if(ticket.Quantity > eventArea.Capacity) return BadRequest("Quantity must be less than or equal to Event Area Capacity");

            if(totalExistingQuantity + ticket.Quantity > eventArea.Capacity) return BadRequest("Total quantity of tickets must be less than or equal to Event Area Capacity");

            Ticket newTicket = new Ticket
            {
                EventAreaID = ticket.EventAreaID,
                TicketName = ticket.TicketName,
                Description = ticket.Description,
                Quantity = ticket.Quantity,
                Price = ticket.Price,
            };

            if (newTicket == null || (eventByeventArea.CreatedBy.ToString() != userId && userRole != "1"))
                return Forbid();

            _logger.LogInformation($"Create new ticket: {newTicket}");

            _context.Tickets.Add(newTicket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = newTicket.TicketID }, newTicket);
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ticket>> UpdateCategory(int id, [FromBody] TicketDto ticket)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (existingTicket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == existingTicket.EventAreaID);
            var eventByeventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);

            if(eventByeventArea == null) return NotFound();

            if (eventByeventArea?.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            existingTicket.TicketName = ticket.TicketName;
            existingTicket.Description = ticket.Description;
            existingTicket.Quantity = ticket.Quantity;
            existingTicket.Price = ticket.Price;

            _logger.LogInformation($"Update ticket: {existingTicket}");

            await _context.SaveChangesAsync();

            return Ok(existingTicket);
        }

        [Authorize(Roles = "1,2")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ticket>> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByeventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);

            if (eventByeventArea.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            _logger.LogInformation($"Delete ticket: {ticket}");

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Ok(ticket);
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("active/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ticket>> ActivateTicket(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByeventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);

            if (eventByeventArea.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            ticket.Status = "Available";

            _logger.LogInformation($"Activate ticket: {ticket}");

            await _context.SaveChangesAsync();

            return Ok(ticket);
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("unactive/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ticket>> DeactivateTicket(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByeventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);

            if (eventByeventArea.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            ticket.Status = "Unavailable";

            _logger.LogInformation($"Deactivate ticket: {ticket}");

            await _context.SaveChangesAsync();

            return Ok(ticket);
        }
    }
}
