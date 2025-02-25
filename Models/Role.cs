using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }

        [MaxLength(50)]
        public string RoleName { get; set; } = String.Empty;

        [MaxLength(150)]
        public string RoleDescription { get; set; } = String.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}