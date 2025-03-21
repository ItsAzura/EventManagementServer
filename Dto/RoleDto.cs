using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class RoleDto
    {
        public string RoleName { get; set; } = String.Empty;
        public string RoleDescription { get; set; } = String.Empty;
    }
}
