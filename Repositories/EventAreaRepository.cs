using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Repositories
{
    public class EventAreaRepository : IEventAreaRepository
    {
        private readonly EventDbContext _context;
        public EventAreaRepository(EventDbContext context)
        {
            _context = context;
        }
        public async Task<EventArea> CreateEventAreaAsync(EventAreaDto eventArea, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (eventArea == null || eventArea.EventID.ToString() != userId && userRole != "1")
                return null;

            EventArea newEventArea = new EventArea
            {
                EventID = eventArea.EventID,
                AreaName = eventArea.AreaName,
                Capacity = eventArea.Capacity,
            };

            _context.EventAreas.Add(newEventArea);
            await _context.SaveChangesAsync();

            return newEventArea;
        }

        public async Task<bool> DeleteEventAreaAsync(int id, ClaimsPrincipal user)
        {
            var eventArea = await _context.EventAreas
                .Include(e => e.Event)
                .FirstOrDefaultAsync(ea => ea.EventAreaID == id);

            if (eventArea == null) return false;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (eventArea.Event.CreatedBy.ToString() != userId && userRole != "1")
                return false;

            _context.EventAreas.Remove(eventArea);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<EventArea?> GetEventAreaByEventIdAsync(int id)
        {
            return await _context.EventAreas
                .FirstOrDefaultAsync(e => e.EventID == id);
        }

        public async Task<EventArea?> GetEventAreaByIdAsync(int id)
        {
            return await _context.EventAreas
                .FirstOrDefaultAsync(e => e.EventAreaID == id);
        }

        public async Task<IEnumerable<EventArea>> GetEventAreasAsync()
        {
            return await _context.EventAreas.ToListAsync();
        }

        public async Task<EventArea> UpdateEventAreaAsync(int id, EventAreaDto eventArea, ClaimsPrincipal user)
        {
            var existingEventArea = await _context.EventAreas.FirstOrDefaultAsync(ea => ea.EventAreaID == id);
            if(existingEventArea == null) return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (existingEventArea.Event.CreatedBy.ToString() != userId && userRole != "1")
                return null;

            existingEventArea.EventID = eventArea.EventID;
            existingEventArea.AreaName = eventArea.AreaName;
            existingEventArea.Capacity = eventArea.Capacity;

            await _context.SaveChangesAsync();
            return existingEventArea;
        }
    }
}
