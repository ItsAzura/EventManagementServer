using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } = String.Empty;

        [Required, MaxLength(150)]
        public string RoleDescription { get; set; } = String.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public required ICollection<User> Users { get; set; }
    }
}