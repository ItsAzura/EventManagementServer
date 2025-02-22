using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int EventID { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [ForeignKey("UserID")]
        public required User User { get; set; }

        [ForeignKey("EventID")]
        public required Event Event { get; set; }

        public required ICollection<RegistrationDetail> RegistrationDetails { get; set; }


    }
}