using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Models
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required, MaxLength(255)]
        public required string Message { get; set; }

        [Required, MaxLength(50)]
        public required string Type { get; set; } // Info, Warning, Success

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserID")]
        public required User User { get; set; }
    }
}
