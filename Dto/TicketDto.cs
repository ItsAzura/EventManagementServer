using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class TicketDto
    {
        public int EventAreaID { get; set; }
        public string TicketName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Unavailable"; // Available,  Unavailable , Sold Out
    }
}
