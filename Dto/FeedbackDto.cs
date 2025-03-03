using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class FeedbackDto
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        [Required, MaxLength(255)]
        public required string Comment { get; set; }
        [Required]
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
