using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; } = DateTime.Now;

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }

    }
}