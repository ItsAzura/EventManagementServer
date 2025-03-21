using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class EventDto
    {
        public string EventName { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public int CreatedBy { get; set; } 
        public IFormFile? EventImageFile { get; set; } 
        public string EventStatus { get; set; } = string.Empty;
    }

}
