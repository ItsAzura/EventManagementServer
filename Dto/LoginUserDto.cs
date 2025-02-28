using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class LoginUserDto
    {
        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
