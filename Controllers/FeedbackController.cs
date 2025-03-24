using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using EventManagementServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("Feedback")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FeedbackController : Controller
    {
        private readonly EventDbContext _context;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(EventDbContext context, ILogger<FeedbackController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("event/{eventId}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Feedback>>  GetFeedBackByEventId (int eventId)
        {
            var feedback = await _context
                .Feedbacks.Where(x => x.EventId == eventId).ToListAsync();

            if (feedback == null) return NotFound();

            _logger.LogInformation($"Get feedback by event id: {feedback}");

            return Ok(feedback);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> CreateFeedback([FromBody] FeedbackDto feedbackDto, [FromServices] IHubContext<NotificationHub> hubContext)
        {
            //Kiểm tra xem user đã đăng nhập chưa
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null) return Unauthorized();

            //Kiểm tra dữ liệu đầu vào
            if (feedbackDto == null) return BadRequest("Feedback data is required");
            if (feedbackDto.Rating < 1 || feedbackDto.Rating > 5) return BadRequest("Rating must be between 1 and 5");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //Kiểm tra xem event có tồn tại không
            var eventInfo = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == feedbackDto.EventId);

            if (eventInfo == null) return NotFound("Event not found");

            var feedback = new Feedback
            {
                UserId = int.Parse(userId),
                EventId = feedbackDto.EventId,
                Comment = feedbackDto.Comment,
                Rating = feedbackDto.Rating,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation($"New feedback from user {userId}");

            _context.Feedbacks.Add(feedback);

            //Gửi thông báo cho người tạo event
            var organizer = await _context.Users.FindAsync(eventInfo.CreatedBy);

            //Nếu tồn tại người tạo event
            if (organizer != null)
            {
                //Tạo thông báo
                var notification = new Notification
                {
                    UserID = organizer.UserID,
                    Message = $"New feedback for event {eventInfo.EventName}",
                    Type = "Feedback",
                    CreatedAt = DateTime.UtcNow

                };

                //Lưu thông báo vào database
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            if (organizer != null)
            {
                //Gửi thông báo tới người tạo event
                await hubContext.Clients.User(organizer.UserID.ToString()).SendAsync("ReceiveNotification", new
                {
                    Title = "New Feedback",
                    Message = $"You receive new responses from the event '{eventInfo.EventName}'!",
                    EventId = eventInfo.EventID,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                message = "Feedback submitted successfully",
                feedbackId = feedback.Id
            });

        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteFeedback(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null) return NotFound();

            if (feedback.UserId.ToString() != userId && userRole != "1") return Forbid();

            _logger.LogInformation($"Delete feedback {id}");

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
