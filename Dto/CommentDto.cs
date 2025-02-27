using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class CommentDto
    {
        [Required]
        public int EventID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
