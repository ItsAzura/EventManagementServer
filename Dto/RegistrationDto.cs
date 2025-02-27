using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class RegistrationDto
    {
        [Required]
        public int UserID { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        public List<RegistrationDetailDto>? RegistrationDetails { get; set; }
    }
}
