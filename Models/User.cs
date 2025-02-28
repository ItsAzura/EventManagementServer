using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string UserName { get; set; } = String.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = String.Empty;

        [Required]
        public int RoleID { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = String.Empty;
        
        public DateTime CreatedAt { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        [ForeignKey("RoleID")]
        public  Role? Role { get; set; }

        public  ICollection<Registration>? Registrations { get; set; }
        public  ICollection<Comment>? Comments { get; set; }
        public  ICollection<Notification>? Notifications { get; set; }
    }
}
