using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class EventCategory
    {
        [Key]
        public int EventCategoryID { get; set; }
        public int EventID { get; set; }

        [ForeignKey("EventID")]
        public required Event Event { get; set; }

        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public required Category Category { get; set; }
    }
}