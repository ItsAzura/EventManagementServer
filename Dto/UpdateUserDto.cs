using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class UpdateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleID { get; set; }
    }
}
