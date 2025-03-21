using EventManagementServer.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Dto
{
    public class EventCategoryDto
    {
        public int EventID { get; set; }

        public int CategoryID { get; set; }
    }
}
