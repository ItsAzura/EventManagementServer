using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EventManagementServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EventDbContext _context;
        public UserRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, int? roleId, string? search, ClaimsPrincipal userClaims)
        {
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "1") throw new UnauthorizedAccessException();
            var query = _context.Users.AsQueryable();
            if (roleId.HasValue)
                query = query.Where(u => u.RoleID == roleId.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.UserName.Contains(search));
            var totalCount = await query.CountAsync();
            var users = await query.OrderByDescending(u => u.UserID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (users, totalCount);
        }

        public async Task<User?> GetUserByIdAsync(int id, ClaimsPrincipal userClaims)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
            var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (user == null) return null;
            if (user.UserID.ToString() != userId && userRole != "1")
                throw new UnauthorizedAccessException();
            return user;
        }

        public async Task<User> CreateUserAsync(UserDto user, IConfiguration configuration, ClaimsPrincipal userClaims)
        {
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "1") throw new UnauthorizedAccessException();
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
            return newUser;
        }

        public async Task<User?> UpdateUserAsync(int id, UpdateUserDto user, ClaimsPrincipal userClaims)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null) return null;
            var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (existingUser.UserID.ToString() != userId && userRole != "1")
                throw new UnauthorizedAccessException();
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
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(int id, ClaimsPrincipal userClaims)
        {
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "1") throw new UnauthorizedAccessException();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeRoleAsync(string role, int id, ClaimsPrincipal userClaims)
        {
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "1") throw new UnauthorizedAccessException();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            user.RoleID = role switch
            {
                "Admin" => 1,
                "User" => 2,
                _ => 2
            };
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 