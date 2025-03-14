using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using EventManagementServer.Services;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : Controller
    {
        private readonly EventDbContext _context;
        private readonly IConfiguration _configuration;

        private readonly EmailService _emailService;

        public ContactController(EventDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<ActionResult<Contact>> PostContact(ContactRequestDto contactRequest)
        {
            if (string.IsNullOrEmpty(contactRequest.Email) || string.IsNullOrEmpty(contactRequest.Name))
            {
                return BadRequest("Email và tên không được để trống");
            }

            var contact = new Contact
            {
                Name = contactRequest.Name,
                Email = contactRequest.Email,
                Message = contactRequest.Message,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            return Ok( );
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            return await _context.Contacts.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        [Authorize(Roles = "1")]
        [HttpPost("respond/{id}")]
        public async Task<IActionResult> RespondToContact(int id, ContactResponseDto response)
        {
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            // Gửi email phản hồi
            string subject = "Phản hồi từ Event Management";
            string body = $"<p>Xin chào {contact.Name},</p>" +
                          $"<p>{response.ResponseMessage}</p>" +
                          $"<p>Trân trọng,<br/>Event Management Team</p>";

            await _emailService.SendEmailAsync(contact.Email, subject, body);

            // Cập nhật trạng thái đã phản hồi
            contact.IsResponded = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã phản hồi thành công" });
        }


    }
}
