using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class EventCategory
    {
        [Key]
        public int EventCategoryID { get; set; }
        [Required]
        public int EventID { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }
    }
}