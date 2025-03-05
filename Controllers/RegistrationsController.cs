using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class RegistrationsController : Controller
    {
        private readonly EventDbContext _context;

        public RegistrationsController(EventDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<Registration>>> GetRegistrations()
        {
            return await _context.Registrations
                .Include(r => r.RegistrationDetails)
                .ThenInclude(rd => rd.Ticket)
                .ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Registration>> GetRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.RegistrationDetails)
                .ThenInclude(rd => rd.Ticket)
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            if (registration == null) return NotFound();

            return Ok(registration);
        }

        [Authorize]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Registration>> CreateRegistration([FromBody] RegistrationDto registrationDto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (registrationDto.UserID.ToString() != userId || userRole != "1")
                return Forbid();

            var ticketIds = registrationDto.RegistrationDetails.Select(rd => rd.TicketID).ToList();

            //Kiểm tra xem các ticket có hợp lệ không
            var validTickets = await _context.Tickets.Where(t => ticketIds.Contains(t.TicketID)).ToListAsync();

            if (validTickets.Count != ticketIds.Count)
                return BadRequest("One or more tickets are invalid.");

            //Kiểm tra xem số lượng đăng ký có lớn hơn 0 không
            if (registrationDto.RegistrationDetails.Any(rd => rd.Quantity <= 0))
                return BadRequest("Quantity must be greater than 0.");

            //Kiểm tra xem số lượng đăng ký có lớn hơn số lượng còn lại không
            foreach (var detail in registrationDto.RegistrationDetails)
            {
                var ticket = validTickets.FirstOrDefault(t => t.TicketID == detail.TicketID);

                if (ticket == null)
                    return BadRequest($"TicketID {detail.TicketID} not found.");

                if (ticket.Quantity < detail.Quantity)
                    return BadRequest($"Not enough tickets for Ticket {detail.TicketID}.");
            }

            var registration = new Registration
            {
                UserID = registrationDto.UserID,
                RegistrationDate = registrationDto.RegistrationDate != DateTime.MinValue
                    ? registrationDto.RegistrationDate.ToUniversalTime()
                    : DateTime.UtcNow, // Nếu không có giá trị hợp lệ, gán thời gian hiện tại
                RegistrationDetails = registrationDto.RegistrationDetails.Select(rd => new RegistrationDetail
                {
                    TicketID = rd.TicketID,
                    Quantity = rd.Quantity
                }).ToList()
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistration), new { id = registration.RegistrationID }, registration);

        }

        [Authorize]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<Registration>> UpdateRegistration(int id, [FromBody] RegistrationDto registrationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var registration = await _context.Registrations
                .Include(r => r.RegistrationDetails)
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            if (registration == null)
                return NotFound($"Registration with ID {id} not found.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (registration.UserID.ToString() != userId || userRole != "1")
                return Forbid();

            var ticketIds = registrationDto.RegistrationDetails.Select(rd => rd.TicketID).ToList();

            // Kiểm tra vé hợp lệ
            var validTickets = await _context.Tickets.Where(t => ticketIds.Contains(t.TicketID)).ToListAsync();
            if (validTickets.Count != ticketIds.Count)
                return BadRequest("One or more tickets are invalid.");

            // 🚫 Kiểm tra số lượng đăng ký
            foreach (var detail in registrationDto.RegistrationDetails)
            {
                var ticket = validTickets.FirstOrDefault(t => t.TicketID == detail.TicketID);
                if (ticket == null)
                    return BadRequest($"TicketID {detail.TicketID} not found.");

                if (detail.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                if (ticket.Quantity < detail.Quantity)
                    return BadRequest($"Not enough tickets for TicketID {detail.TicketID}.");
            }

            // Cập nhật thông tin cơ bản
            registration.UserID = registrationDto.UserID;
            registration.RegistrationDate = registrationDto.RegistrationDate != DateTime.MinValue
                ? registrationDto.RegistrationDate.ToUniversalTime()
                : DateTime.UtcNow; // Nếu không có giá trị hợp lệ, gán thời gian hiện tại

            // Cập nhật chi tiết đăng ký
            foreach (var detailDto in registrationDto.RegistrationDetails)
            {
                var existingDetail = registration.RegistrationDetails.FirstOrDefault(d => d.TicketID == detailDto.TicketID);

                if (existingDetail != null)
                {
                    // Cập nhật số lượng nếu đã tồn tại
                    existingDetail.Quantity = detailDto.Quantity;
                }
                else
                {
                    // Thêm mới nếu chưa có
                    registration.RegistrationDetails.Add(new RegistrationDetail
                    {
                        TicketID = detailDto.TicketID,
                        Quantity = detailDto.Quantity
                    });
                }
            }

            // Xóa các chi tiết không còn trong DTO
            var detailsToRemove = registration.RegistrationDetails
                .Where(rd => !registrationDto.RegistrationDetails.Any(d => d.TicketID == rd.TicketID))
                .ToList();

            foreach (var detail in detailsToRemove)
            {
                _context.RegistrationDetails.Remove(detail);
            }

            await _context.SaveChangesAsync();

            return Ok(registration);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.RegistrationDetails)
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (registration == null)
                return NotFound($"Registration with ID {id} not found.");

            if (registration.UserID.ToString() != userId || userRole != "1")
                return Forbid();

            var deletedRegistration = registration; // Lưu thông tin đăng ký bị xóa

            _context.RegistrationDetails.RemoveRange(registration.RegistrationDetails);

            _context.Registrations.Remove(registration);

            await _context.SaveChangesAsync();

            return Ok(deletedRegistration); // Trả về dữ liệu đã xóa

        }



    }
}
