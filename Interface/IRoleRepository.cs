using EventManagementServer.Dto;
using EventManagementServer.Models;
using Microsoft.AspNetCore.Mvc;


namespace EventManagementServer.Interface
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetRoles();

        Task<Role> GetRoleById(int id);

        Task<Role> CreateRole(RoleDto role);

        Task<Role> UpdateRole(int id, RoleDto role);

        Task<bool> DeleteRole(int id);
    }
}
