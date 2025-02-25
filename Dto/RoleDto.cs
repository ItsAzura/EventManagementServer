using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class RoleDto
    {
        [Required, MaxLength(50)]
        public string RoleName { get; set; } = String.Empty;

        [Required, MaxLength(150)]
        public string RoleDescription { get; set; } = String.Empty;
    }
}
