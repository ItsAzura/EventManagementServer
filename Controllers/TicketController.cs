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
    public class TicketController : Controller
    {
        private readonly EventDbContext _context;

        public TicketController(EventDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
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

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Tickets = tickets
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Ticket>> GetTicketById(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            return Ok(ticket);
        }

        [HttpGet("eventarea/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<EventArea>>> GetTicketByEventAreaId(int id)
        {
            var ticket = await _context.Tickets
                .Where(t => t.EventAreaID == id)
                .ToListAsync();

            if (ticket == null) return NotFound();

            return Ok(ticket);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Ticket>> CreateTicket([FromBody] TicketDto ticket)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);

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

            if (newTicket == null || (newTicket.EventArea?.Event?.CreatedBy.ToString() != userId && userRole != "1"))
                return Forbid();

            _context.Tickets.Add(newTicket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = newTicket.TicketID }, newTicket);
        }

        [Authorize]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Ticket>> UpdateCategory(int id, [FromBody] TicketDto ticket)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (existingTicket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existingTicket.EventArea?.Event?.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            existingTicket.EventAreaID = ticket.EventAreaID;
            existingTicket.TicketName = ticket.TicketName;
            existingTicket.Description = ticket.Description;
            existingTicket.Quantity = ticket.Quantity;
            existingTicket.Price = ticket.Price;
            existingTicket.Status = ticket.Status;

            await _context.SaveChangesAsync();

            return Ok(existingTicket);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Ticket>> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (ticket.EventArea?.Event?.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Ok(ticket);
        }
    }
}
