using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly EventDbContext _context;
        private readonly IConfiguration configuration;

        public UserController(EventDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(int page = 1, int pageSize = 10, int? RoleId = null, string? search = null)
        {
            if(page < 1 || pageSize < 1) return BadRequest("Invalid page or pageSize");

            var query = _context.Users.AsQueryable();

            if(RoleId.HasValue)
            {
                query = query.Where(u => u.RoleID == RoleId.Value);
            }

            if(!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Users = users
            };

            return Ok(response);
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id); 

            if (user == null) return NotFound();

            return Ok(user);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserDto user)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            // Hash the password with the salt
            string hashPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: user.PasswordHash,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            ));

            User newUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
                RoleID = configuration.GetValue<int>("User"),
                PasswordHash = hashPassword
            };

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserID }, newUser);
        }

        [Authorize(Roles = "1")]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] UpdateUserDto user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _context.Users.FindAsync(id);
             
            if(existingUser == null) return BadRequest();
     
            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            // Hash the password with the salt
            string hashPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: user.PasswordHash,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256/8
            ));

            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.RoleID = user.RoleID;
            existingUser.PasswordHash = hashPassword;

            _context.Entry(existingUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if(user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
