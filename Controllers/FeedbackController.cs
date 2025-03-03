using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using EventManagementServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly EventDbContext _context;

        public FeedbackController(EventDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<Feedback>>  GetFeedBackByEventId (int eventId)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.EventId == eventId);

            if (feedback == null) return NotFound();

            return Ok(feedback);
        }

        [Authorize]
        [HttpPost]
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
    }
}
