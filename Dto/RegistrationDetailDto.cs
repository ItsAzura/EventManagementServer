using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class RegistrationDetailDto
    {
        [Required]
        public int TicketID { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
