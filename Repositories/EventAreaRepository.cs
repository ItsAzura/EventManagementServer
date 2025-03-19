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
            if (eventArea == null)
                return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(
                e => e.EventID == eventArea.EventID
                );

            if (!int.TryParse(userIdClaim, out int userId))
                return null; // Trả về null nếu userId không hợp lệ

            int userIdInt = Convert.ToInt32(userId);
            if (eventByEventArea.CreatedBy != userId && userRole != "1")
                return null; // Chỉ chủ sự kiện hoặc admin mới có quyền

            try
            {
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
            catch (Exception ex)
            {
                // Ghi log lỗi để debug
                Console.WriteLine($"Error creating event area: {ex.Message}");
                return null;
            }
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

        public async Task<IEnumerable<EventArea>> GetEventAreaByEventIdAsync(int id)
        {
            return await _context.EventAreas
               .Where(e => e.EventID == id)
               .ToListAsync();
        }

        public async Task<EventArea> GetEventAreaByIdAsync(int id)
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

            var eventByEventArea = await _context.Events.FirstOrDefaultAsync(e => e.EventID == existingEventArea.EventID);

            if (eventByEventArea.CreatedBy.ToString() != userId && userRole != "1") return null;

            existingEventArea.AreaName = eventArea.AreaName;
            existingEventArea.Capacity = eventArea.Capacity;

            await _context.SaveChangesAsync();
            return existingEventArea;
        }
    }
}
