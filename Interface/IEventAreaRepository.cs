using EventManagementServer.Dto;
using EventManagementServer.Models;
using System.Security.Claims;

namespace EventManagementServer.Interface
{
    public interface IEventAreaRepository
    {
        Task<IEnumerable<EventArea>> GetEventAreasAsync();
        Task<EventArea> GetEventAreaByIdAsync(int id);
        Task<IEnumerable<EventArea>> GetEventAreaByEventIdAsync(int id);
        Task<EventArea> CreateEventAreaAsync(EventAreaDto eventArea, ClaimsPrincipal user);
        Task<EventArea> UpdateEventAreaAsync(int id, EventAreaDto eventArea, ClaimsPrincipal user);
        Task<bool> DeleteEventAreaAsync(int id, ClaimsPrincipal user);
    }
}
