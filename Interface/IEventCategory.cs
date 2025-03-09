using EventManagementServer.Dto;
using EventManagementServer.Models;
using System.Security.Claims;

namespace EventManagementServer.Interface
{
    public interface IEventCategoryRepository
    {
        Task<EventCategory> CreateEventCategoryAsync(EventCategoryDto eventCategory, ClaimsPrincipal user);
        Task<bool> DeleteEventCategoryAsync(int id, ClaimsPrincipal user);
        Task<IEnumerable<EventCategory>> GetEventCategoriesAsync();
        Task<EventCategory?> GetEventCategoryByIdAsync(int id);
        Task<EventCategory?> GetEventCategoryByEventIdAsync(int id);
        Task<EventCategory> UpdateEventCategoryAsync(int id, EventCategoryDto eventCategory, ClaimsPrincipal user);
    }
}
