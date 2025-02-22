using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class EventArea
    {
        [Key]
        public int EventAreaID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required, MaxLength(100)]
        public string AreaName { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [ForeignKey("EventID")]
        public required Event Event { get; set; }

        public required ICollection<Ticket> Tickets { get; set; }
    }
}
