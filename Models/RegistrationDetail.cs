using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace EventManagementServer.Models
{
    public class RegistrationDetail
    {
        [Key]
        public int RegistrationDetailID { get; set; }

        [Required]
        public int RegistrationID { get; set; }

        [Required]
        public int TicketID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [ForeignKey("RegistrationID")]
        public Registration? Registration { get; set; }

        [ForeignKey("TicketID")]
        public Ticket? Ticket { get; set; }

    }
}