using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class EventDto
    {
        [Required(ErrorMessage = "Event Name is required")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event Description is required")]
        public string EventDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event Date is required")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Event Location is required")]
        public string EventLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; } 

        [Required(ErrorMessage = "Event Image is required")]
        public IFormFile? EventImageFile { get; set; } 
    }

}
