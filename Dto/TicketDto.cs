using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class TicketDto
    {
        [Required]
        public int EventAreaID { get; set; }

        [Required, MaxLength(100)]
        public string TicketName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Unavailable"; // Available,  Unavailable , Sold Out
    }
}
