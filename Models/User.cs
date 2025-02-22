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
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("RoleID")]
        public required Role Role { get; set; }

        public required ICollection<Registration> Registrations { get; set; }
        public required ICollection<Comment> Comments { get; set; }
        public required ICollection<Notification> Notifications { get; set; }
    }
}
