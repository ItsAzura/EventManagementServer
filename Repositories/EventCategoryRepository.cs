using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Repositories
{
    public class EventCategoryRepository : IEventCategoryRepository
    {
        private readonly EventDbContext _context;
        public EventCategoryRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<EventCategory> CreateEventCategoryAsync(EventCategoryDto eventCategory, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            EventCategory newEventCategory = new EventCategory
            {
                EventID = eventCategory.EventID,
                CategoryID = eventCategory.CategoryID,
            };

            //if (newEventCategory.Event == null || (newEventCategory.Event.CreatedBy.ToString() != userId && userRole != "1")) return null;

            _context.EventCategories.Add(newEventCategory);
            await _context.SaveChangesAsync();

            return newEventCategory;
        }

        public async Task<bool> DeleteEventCategoryAsync(int id, ClaimsPrincipal user)
        {
            var eventCategory = await _context.EventCategories
               .Include(ec => ec.Event)
               .FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (eventCategory == null) return false;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (eventCategory.Event.CreatedBy.ToString() != userId && userRole != "1")
                return false;

            _context.EventCategories.Remove(eventCategory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<EventCategory>> GetEventCategoriesAsync()
        {
            return await _context.EventCategories.ToListAsync();
        }

        public async Task<EventCategory?> GetEventCategoryByEventIdAsync(int id)
        {
            return await _context.EventCategories
                .Include(ec => ec.Event)
                .Include(ec => ec.Category)
                .FirstOrDefaultAsync(ec => ec.EventID == id);
        }

        public async Task<EventCategory?> GetEventCategoryByIdAsync(int id)
        {
            return await _context.EventCategories
                .Include(ec => ec.Event)
                .Include(ec => ec.Category)
                .FirstOrDefaultAsync(ec => ec.EventCategoryID == id);
        }

        public async Task<EventCategory> UpdateEventCategoryAsync(int id, EventCategoryDto eventCategory, ClaimsPrincipal user)
        {
            var existingEventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.EventCategoryID == id);

            if (existingEventCategory == null) return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            //var eventByEventCategory = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventCategory.EventID);

            //if (eventByEventCategory?.Creator?.UserID.ToString() != userId && userRole != "1") return null;

            existingEventCategory.EventID = eventCategory.EventID;
            existingEventCategory.CategoryID = eventCategory.CategoryID;

            await _context.SaveChangesAsync();
            return existingEventCategory;
        }
    }
}
