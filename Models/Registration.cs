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

        public DateTime RegistrationDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }

        public ICollection<RegistrationDetail>? RegistrationDetails { get; set; }

    }
}