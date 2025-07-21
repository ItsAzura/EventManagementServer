using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly EventDbContext _context;
        public TicketRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetTicketsAsync(int page, int pageSize, int? quantity, decimal? price, string? search)
        {
            var query = _context.Tickets.AsQueryable();
            if (quantity.HasValue)
                query = query.Where(t => t.Quantity == quantity.Value);
            if (price.HasValue)
                query = query.Where(t => t.Price == price.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.TicketName.Contains(search));
            var totalCount = await query.CountAsync();
            var tickets = await query.OrderBy(t => t.TicketID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (tickets, totalCount);
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEventAreaIdAsync(int eventAreaId)
        {
            return await _context.Tickets.Where(t => t.EventAreaID == eventAreaId).ToListAsync();
        }

        public async Task<Ticket> CreateTicketAsync(TicketDto ticket, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);
            if (eventArea == null) throw new Exception("Event Area not found");
            var totalExistingQuantity = await _context.Tickets.Where(t => t.EventAreaID == ticket.EventAreaID).SumAsync(t => (int?)t.Quantity) ?? 0;
            if (ticket.Quantity > eventArea.Capacity) throw new Exception("Quantity must be less than or equal to Event Area Capacity");
            if (totalExistingQuantity + ticket.Quantity > eventArea.Capacity) throw new Exception("Total quantity of tickets must be less than or equal to Event Area Capacity");
            Ticket newTicket = new Ticket
            {
                EventAreaID = ticket.EventAreaID,
                TicketName = ticket.TicketName,
                Description = ticket.Description,
                Quantity = ticket.Quantity,
                Price = ticket.Price,
            };
            if (newTicket == null || (eventByEventArea.CreatedBy.ToString() != userId && userRole != "1"))
                throw new UnauthorizedAccessException();
            _context.Tickets.Add(newTicket);
            await _context.SaveChangesAsync();
            return newTicket;
        }

        public async Task<Ticket?> UpdateTicketAsync(int id, TicketDto ticket, ClaimsPrincipal user)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
            if (existingTicket == null) return null;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == existingTicket.EventAreaID);
            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);
            if (eventByEventArea == null) return null;
            if (eventByEventArea.CreatedBy.ToString() != userId && userRole != "1")
                throw new UnauthorizedAccessException();
            existingTicket.TicketName = ticket.TicketName;
            existingTicket.Description = ticket.Description;
            existingTicket.Quantity = ticket.Quantity;
            existingTicket.Price = ticket.Price;
            await _context.SaveChangesAsync();
            return existingTicket;
        }

        public async Task<bool> DeleteTicketAsync(int id, ClaimsPrincipal user)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
            if (ticket == null) return false;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);
            if (eventByEventArea.CreatedBy.ToString() != userId && userRole != "1")
                throw new UnauthorizedAccessException();
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Ticket?> ActivateTicketAsync(int id, ClaimsPrincipal user)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
            if (ticket == null) return null;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);
            if (eventByEventArea.CreatedBy.ToString() != userId && userRole != "1")
                throw new UnauthorizedAccessException();
            ticket.Status = "Available";
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket?> DeactivateTicketAsync(int id, ClaimsPrincipal user)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
            if (ticket == null) return null;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var eventArea = await _context.EventAreas.FirstOrDefaultAsync(e => e.EventAreaID == ticket.EventAreaID);
            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventArea.EventID);
            if (eventByEventArea.CreatedBy.ToString() != userId && userRole != "1")
                throw new UnauthorizedAccessException();
            ticket.Status = "Unavailable";
            await _context.SaveChangesAsync();
            return ticket;
        }
    }
} 