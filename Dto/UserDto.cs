using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class UserDto
    {
        [Required, MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int RoleID { get; set; }

    }
}
