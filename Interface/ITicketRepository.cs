using EventManagementServer.Dto;
using EventManagementServer.Models;
using System.Security.Claims;

namespace EventManagementServer.Interface
{
    public interface ITicketRepository
    {
        Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetTicketsAsync(int page, int pageSize, int? quantity, decimal? price, string? search);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsByEventAreaIdAsync(int eventAreaId);
        Task<Ticket> CreateTicketAsync(TicketDto ticket, ClaimsPrincipal user);
        Task<Ticket?> UpdateTicketAsync(int id, TicketDto ticket, ClaimsPrincipal user);
        Task<bool> DeleteTicketAsync(int id, ClaimsPrincipal user);
        Task<Ticket?> ActivateTicketAsync(int id, ClaimsPrincipal user);
        Task<Ticket?> DeactivateTicketAsync(int id, ClaimsPrincipal user);
    }
} 