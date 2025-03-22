using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly EventDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;

        public UserController(EventDbContext context, IConfiguration configuration, ILogger<UserController> logger)
        {
            _context = context;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
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
                .OrderByDescending(u => u.UserID)
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

            _logger.LogInformation($"Get users: {response}");

            return Ok(response);
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id); 

            if (user == null) return NotFound();

            _logger.LogInformation($"Get user by id: {user}");

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
                RoleID = _configuration.GetValue<int>("User"),
                PasswordHash = hashPassword
            };

            _logger.LogInformation($"Create new user: {newUser}");

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserID }, newUser);
        }

        [Authorize(Roles = "1,2")]
        [HttpPut("{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] UpdateUserDto user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _context.Users.FindAsync(id);
             
            if(existingUser == null) return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existingUser.UserID.ToString() != userId && userRole != "1")
                return Forbid();

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

            _logger.LogInformation($"Update user: {existingUser}");

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

            if(user.RoleID == '1')
            {
                return BadRequest("Cannot delete an admin user");
            }

            _logger.LogInformation($"Delete user: {user}");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "1")]
        [HttpPatch("ChangeRole/{id}")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<ActionResult> ChangeRole(string role, int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound();

            user.RoleID = role switch
            {
                "Admin" => 1,
                "User" => 2,
                _ => 2
            };

            _logger.LogInformation($"Change role: {user}");

            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
