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
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        private readonly EventDbContext _context;

        public EventController(EventDbContext context)
        {
            _context = context;
        }

        [HttpGet("images/{imageName}")]
        public IActionResult GetEventImage(string imageName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            var imageFileStream = System.IO.File.OpenRead(imagePath);
            return File(imageFileStream, "image/jpeg"); // Điều chỉnh kiểu MIME tùy theo loại ảnh
        }


        [HttpGet("/top6")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Event>>> GetTop6Events()
        {
            var events = await _context.Events
                .Where(e => e.EventStatus == "Approved")
                .OrderByDescending(e => e.EventDate)
                .Take(6)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(int page = 1, int pageSize = 10, int? categoryId = null, string? search = null)
        {
            if (page < 1 || pageSize < 1) return BadRequest("Invalid page or pageSize");

            // Query cơ bản
            var query = _context.Events.AsQueryable();

            // Lọc theo Category nếu có
            if (categoryId.HasValue)
            {
                query = query.Where(e => _context.EventCategories // Lọc theo EventCategory
                    .Where(ec => ec.CategoryID == categoryId.Value) // Lọc theo CategoryID
                    .Select(ec => ec.EventID) // Lấy ra EventID
                    .Contains(e.EventID)); // Kiểm tra EventID có nằm trong danh sách EventID của Category không
            }

            //Tìm theo title nếu có
            if(!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.EventName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var events = await query
                .Where(e => e.EventStatus == "Approved") // Lọc theo trạng thái
                .OrderBy(e => e.EventID)  // Sắp xếp theo ID (hoặc tùy chỉnh)
                .Skip((page - 1) * pageSize) // Bỏ qua các phần tử trước đó
                .Take(pageSize) // Lấy số phần tử cần
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Events = events
            };

            return Ok(response);
        }

        [HttpGet("admin")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEventsAdmin(int page = 1, int pageSize = 10, int? categoryId = null, string? search = null)
        {
            if (page < 1 || pageSize < 1) return BadRequest("Invalid page or pageSize");

            // Query cơ bản
            var query = _context.Events.AsQueryable();

            // Lọc theo Category nếu có
            if (categoryId.HasValue)
            {
                query = query.Where(e => _context.EventCategories // Lọc theo EventCategory
                    .Where(ec => ec.CategoryID == categoryId.Value) // Lọc theo CategoryID
                    .Select(ec => ec.EventID) // Lấy ra EventID
                    .Contains(e.EventID)); // Kiểm tra EventID có nằm trong danh sách EventID của Category không
            }

            //Tìm theo title nếu có
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.EventName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var events = await query
                .OrderBy(e => e.EventID)  // Sắp xếp theo ID (hoặc tùy chỉnh)
                .Skip((page - 1) * pageSize) // Bỏ qua các phần tử trước đó
                .Take(pageSize) // Lấy số phần tử cần
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Events = events
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var _event = await _context.Events
                .Where(ea => ea.EventID == id && ea.EventStatus == "Approved")
                .ToListAsync();

            if (_event == null) return NotFound();

            return Ok(_event);
        }

        [HttpGet("user/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Event>> GetEventByUserId(int id)
        {
            var _event = await _context.Events
                .Where(ea => ea.CreatedBy == id && ea.EventStatus == "Approved")
                .FirstOrDefaultAsync(e => e.CreatedBy == id);

            if (_event == null) return NotFound();
            
            return Ok(_event);
        }

        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")] //Kiểu request body là form-data 
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Event>> CreateEvent([FromForm] EventDto _event, [FromServices] IHubContext<NotificationHub> hubContext)
        {
            //Kiểm tra xem EventImageFile có tồn tại hay không
            if (_event.EventImageFile == null)
            {
                ModelState.AddModelError("EventImageFile", "Event Image is required");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (_event == null) return BadRequest("Invalid event data.");
            if (_event.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();


            //Tạo thư mục Images nếu chưa tồn tại
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            //Tạo tên mới cho file ảnh
            string newFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(_event.EventImageFile.FileName)}";
            string imagePath = Path.Combine(folderPath, newFileName);

            //Lưu file ảnh vào thư mục Images
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await _event.EventImageFile.CopyToAsync(stream);
            }

            //Tạo sự kiện mới
            var newEvent = new Event
            {
                EventName = _event.EventName,
                EventDescription = _event.EventDescription,
                EventDate = _event.EventDate.ToUniversalTime(),
                EventLocation = _event.EventLocation,
                EventImage = newFileName,
                CreatedBy = _event.CreatedBy,
                EventStatus = "Pending",
            };

            _context.Events.Add(newEvent);

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.RoleID == 1);

            //Tạo thông báo cho admin
            if(admin != null)
            {
                var notification = new Notification
                {
                    UserID = admin.UserID,
                    Message = $"There is a new event arrangements forum: {newEvent.EventName}",
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            if (admin != null) {
                await hubContext.Clients.User(admin.UserID.ToString()).SendAsync("ReceiveNotification", new
                {
                    Title = "New Event",
                    Message = $"There is a new event arrangements forum: {newEvent.EventName}",
                    EventId = newEvent.EventID,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventID }, newEvent);
        }

        [Authorize(Roles = "1")]
        [HttpPut("{id}/approve")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> ApproveEvent(int id, [FromServices] IHubContext<NotificationHub> hubContext)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if(existingEvent == null) return NotFound();

            //Thay đổi trạng thái của sự kiện
            existingEvent.EventStatus = "Approved";

            //Lấy danh sách tất cả người dùng
            var users = await _context.Users.ToListAsync();

            //Lấy thông tin người tạo sự kiện
            var eventCreator = users.FirstOrDefault(u => u.UserID == existingEvent.CreatedBy);

            var notifications = new List<Notification>();

            //Tạo thông báo cho người tạo sự kiện
            if (eventCreator != null)
            {
                notifications.Add(new Notification {
                    UserID = eventCreator.UserID,
                    Message = $"Event {existingEvent.EventName} has been approved",
                    Type = "Success",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                await hubContext.Clients.User(eventCreator.UserID.ToString()).SendAsync("ReceiveNotification", new
                {
                    Title = "Event Approved",
                    Message = $"Event {existingEvent.EventName} has been approved",
                    EventId = existingEvent.EventID,
                    CreatedAt = DateTime.UtcNow
                });
            }

            //Tạo thông báo cho tất cả người dùng
            var otherUsers = users.Where(u => u.UserID != existingEvent.CreatedBy).ToList();
            foreach(var user in otherUsers)
            {
                notifications.Add(new Notification
                {
                    UserID = user.UserID,
                    Message = $"There is a new event arrangements forum: {existingEvent.EventName}",
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            //Lưu thông báo vào database
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            //Gửi thông báo đến tất cả người dùng trừ người tạo sự kiện
            await hubContext.Clients.AllExcept(eventCreator?.UserID.ToString()).SendAsync(
                "ReceiveNotification",
                new
                {
                    Title = "New Event",
                    Message = $"There is a new event arrangements forum: {existingEvent.EventName}",
                    EventId = existingEvent.EventID,
                    CreatedAt = DateTime.UtcNow
                }
            );

            return Ok(new
            {
                success = true,
                message = "Event Approved",
                eventId = existingEvent.EventID
            });
        }

        [Authorize(Roles = "1")]
        [HttpPut("{id}/reject")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> RejectEvent(int id, [FromServices] IHubContext<NotificationHub> hubContext)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if(existingEvent == null) return NotFound();

            existingEvent.EventStatus = "Rejected";

            var users = await _context.Users.ToListAsync();

            var eventCreator = users.FirstOrDefault(u => u.UserID == existingEvent.CreatedBy);

            if (eventCreator != null)
            {
                var notification = new Notification
                {
                    UserID = eventCreator.UserID,
                    Message = $"Event {existingEvent.EventName} has been rejected",
                    Type = "Error",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();


            if (eventCreator != null)
            {
                await hubContext.Clients.User(eventCreator.UserID.ToString())
                   .SendAsync("ReceiveNotification", new
                   {
                       Title = "Event Rejected",
                       Message = $"Event {existingEvent.EventName} has been rejected",
                       EventId = existingEvent.EventID,
                       CreatedAt = DateTime.UtcNow
                   });
            }

            return Ok(new
            {
                success = true,
                message = "Event Rejected",
                eventId = existingEvent.EventID
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Event>> UpdateEvent(int id, [FromForm] EventDto _event, [FromServices] IHubContext<NotificationHub> hubContext)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if(existingEvent == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existingEvent.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            //Tạo thư mục Images nếu chưa tồn tại
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            //Tạo tên mới cho file ảnh
            string newFileName = existingEvent.EventImage;

            //Nếu có file ảnh mới được gửi lên thì lưu file ảnh mới và xóa file ảnh cũ
            if(_event.EventImageFile != null)
            {
                newFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(_event.EventImageFile.FileName)}";

                string imagePath = Path.Combine(folderPath, newFileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await _event.EventImageFile.CopyToAsync(stream);
                }

                var existingImagePath = Path.Combine(folderPath, existingEvent.EventImage);
                if(System.IO.File.Exists(existingImagePath))
                {
                    System.IO.File.Delete(existingImagePath);
                }
            }

            existingEvent.EventName = _event.EventName;
            existingEvent.EventDescription = _event.EventDescription;
            existingEvent.EventDate = _event.EventDate.ToUniversalTime();
            existingEvent.EventLocation = _event.EventLocation;
            existingEvent.EventImage = newFileName;
            existingEvent.CreatedBy = _event.CreatedBy;

            var users = await _context.Users.ToListAsync();
            var notifications = new List<Notification>();

            foreach (var user in users)
            {
                notifications.Add(new Notification
                {
                    UserID = user.UserID,
                    Message = $"There are some changes of events: {existingEvent.EventName}",
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            await hubContext.Clients.All.SendAsync(
                "ReceiveNotification",
                new {
                    Title = "New Event",
                    Message = $"There are some changes of events: {existingEvent.EventName}",
                    EventId = existingEvent.EventID,
                    CreatedAt = DateTime.UtcNow
                });

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if(existingEvent == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existingEvent.CreatedBy.ToString() != userId && userRole != "1")
                return Forbid();

            string imagePath = "Images/" + existingEvent.EventImage;
            if(System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.Events.Remove(existingEvent);
            _context.SaveChanges();
            return Ok();
        }


    }
}
