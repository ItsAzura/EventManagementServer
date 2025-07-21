using EventManagementServer.Controllers;
using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Interface;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly EventDbContext _context;
        
        public RoleRepository(EventDbContext context)
        {
            _context = context;
            
        }
        public async Task<Role> CreateRole(RoleDto role)
        {
            Role newRole = new Role
            {
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
            };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();
            return newRole;

        }

        public async Task<bool> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Role?> GetRoleById(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);
        }

        public async Task<IEnumerable<Role>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role?> UpdateRole(int id, RoleDto role)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);

            if (existingRole == null) return null;

            existingRole.RoleName = role.RoleName;
            existingRole.RoleDescription = role.RoleDescription;

            await _context.SaveChangesAsync();

            return existingRole;
        }
    }
}
