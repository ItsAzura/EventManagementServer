using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace EventManagementServer.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required, MaxLength(50)]
        public string EventName { get; set; } = String.Empty;

        public string EventDescription { get; set; } = String.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string EventLocation { get; set; } = String.Empty;

        [MaxLength(255)]
        public string EventImage { get; set; } = String.Empty;

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("CreatedBy")]
        public  User? Creator { get; set; }

        public  ICollection<EventCategory>? EventCategories { get; set; }
        public  ICollection<Registration>? Registrations { get; set; }
        public  ICollection<EventArea>? EventAreas { get; set; }
        public  ICollection<Comment>? Comments { get; set; }
    }
}
