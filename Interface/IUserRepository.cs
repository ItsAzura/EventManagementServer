using EventManagementServer.Dto;
using EventManagementServer.Models;
using System.Security.Claims;

namespace EventManagementServer.Interface
{
    public interface IUserRepository
    {
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, int? roleId, string? search, ClaimsPrincipal user);
        Task<User?> GetUserByIdAsync(int id, ClaimsPrincipal user);
        Task<User> CreateUserAsync(UserDto user, IConfiguration configuration, ClaimsPrincipal userClaims);
        Task<User?> UpdateUserAsync(int id, UpdateUserDto user, ClaimsPrincipal userClaims);
        Task<bool> DeleteUserAsync(int id, ClaimsPrincipal userClaims);
        Task<bool> ChangeRoleAsync(string role, int id, ClaimsPrincipal userClaims);
    }
} 