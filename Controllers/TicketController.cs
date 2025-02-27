using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            return await _context.Tickets.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicketById(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }

        [HttpGet("eventarea/{id}")]
        public async Task<ActionResult<Ticket>> GetTicketByEventAreaId(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.EventAreaID == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket([FromBody] TicketDto ticket)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);

            if (eventArea == null)
            {
                ModelState.AddModelError("EventAreaID", "Event Area not found");
                return BadRequest(ModelState);
            }

            var totalExistingQuantity = await _context.Tickets
                .Where(t => t.EventAreaID == ticket.EventAreaID)
                .SumAsync(t =>(int?) t.Quantity) ?? 0;

            if(ticket.Quantity > eventArea.Capacity)
            {
                return BadRequest("Quantity must be less than or equal to Event Area Capacity");
            }

            if(totalExistingQuantity + ticket.Quantity > eventArea.Capacity)
            {
                return BadRequest("Total quantity of tickets must be less than or equal to Event Area Capacity");
            }

            Ticket newTicket = new Ticket
            {
                EventAreaID = ticket.EventAreaID,
                TicketName = ticket.TicketName,
                Description = ticket.Description,
                Quantity = ticket.Quantity,
                Price = ticket.Price,
            };

            _context.Tickets.Add(newTicket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = newTicket.TicketID }, newTicket);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Ticket>> UpdateCategory(int id, [FromBody] TicketDto ticket)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (existingTicket == null)
            {
                return NotFound();
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            existingTicket.EventAreaID = ticket.EventAreaID;
            existingTicket.TicketName = ticket.TicketName;
            existingTicket.Description = ticket.Description;
            existingTicket.Quantity = ticket.Quantity;
            existingTicket.Price = ticket.Price;
            existingTicket.Status = ticket.Status;

            await _context.SaveChangesAsync();

            return Ok(existingTicket);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Ticket>> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Ok(ticket);
        }
    }
}
