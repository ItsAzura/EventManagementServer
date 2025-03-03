using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        [Required, MaxLength(255)]
        public required string Comment{ get; set; }
        [Required]
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }
        [ForeignKey("EventID")]
        public Event? Event { get; set; }
    }
}
