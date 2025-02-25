using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = String.Empty;

        public string CategoryDescription { get; set; } = String.Empty;

        public DateTime CreatedAt { get; set; }

        public  ICollection<EventCategory>? EventCategories { get; set; }
    }
}
