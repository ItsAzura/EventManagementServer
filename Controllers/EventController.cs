using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
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
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var _event = await _context.Events
                .Where(ea => ea.EventID == id)
                .ToListAsync();

            if (_event == null) return NotFound();

            return Ok(_event);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<Event>> GetEventByUserId(int id)
        {
            var _event = await _context.Events
                .Where(ea => ea.CreatedBy == id)
                .FirstOrDefaultAsync(e => e.CreatedBy == id);

            if (_event == null) return NotFound();
            
            return Ok(_event);
        }

        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")] //Kiểu request body là form-data 
        public async Task<ActionResult<Event>> CreateEvent([FromForm] EventDto _event)
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

            if (_event == null || (_event.CreatedBy.ToString() != userId && userRole != "1" ))
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

            var newEvent = new Event
            {
                EventName = _event.EventName,
                EventDescription = _event.EventDescription,
                EventDate = _event.EventDate.ToUniversalTime(),
                EventLocation = _event.EventLocation,
                EventImage = newFileName,
                CreatedBy = _event.CreatedBy
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventID }, newEvent);
        }

        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Event>> UpdateEvent(int id, [FromForm] EventDto _event)
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

            _context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
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
