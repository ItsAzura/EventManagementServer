using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = String.Empty;
        [Required, MaxLength(100)]
        public string Email { get; set; } = String.Empty;
        [Required, MaxLength(100)] 
        public string Message { get; set; } = String.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsResponded { get; set; } = false;
    }
}
