using EventManagementServer.Data;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly EventDbContext _context;
        private readonly ILogger<NotificationController> _logger;
        public NotificationController(EventDbContext context, ILogger<NotificationController> logger) { 
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("user/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByUser(int id)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserID == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != id.ToString() && userRole != "1") return Unauthorized();

            _logger.LogInformation($"Get notifications by user id: {notifications}");

            return Ok(notifications);
        }

        [Authorize]
        [HttpGet("unread")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUnreadNotifications()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var notifications = await _context.Notifications
                .Where(n => n.UserID == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            _logger.LogInformation($"Get unread notifications: {notifications}");

            return Ok(notifications);
        }

        [Authorize]
        [HttpGet("read")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetReadNotifications([FromBody] int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);

            if(notification == null) return NotFound();

            notification.IsRead = true;

            _logger.LogInformation($"Read notification: {notification}");
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
