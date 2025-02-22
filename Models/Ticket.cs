using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class Ticket
    {
        [Key]
        public int TicketID { get; set; }

        [Required]
        public int EventAreaID { get; set; }

        [Required, MaxLength(100)]
        public string TicketName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Available"; // Available, Booked, Sold

        [ForeignKey("EventAreaID")]
        public required EventArea EventArea { get; set; }

    }
}