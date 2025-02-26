using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var _event = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if(_event == null)
            {
                return NotFound();
            }

            return Ok(_event);
        }

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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Event>> UpdateEvent(int id, [FromForm] EventDto _event)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if(existingEvent == null)
            {
                return NotFound();
            }

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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if(existingEvent == null)
            {
                return NotFound();
            }

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
