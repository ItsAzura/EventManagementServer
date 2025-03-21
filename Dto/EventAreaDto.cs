using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class EventAreaDto
    {
        public int EventID { get; set; }

        public string AreaName { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }
    }
}
