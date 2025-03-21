using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class NotificationDto
    {
        public int UserID { get; set; }

        public required string Message { get; set; }

        public required string Type { get; set; } // Info, Warning, Success

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
