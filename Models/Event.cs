﻿using System.ComponentModel.DataAnnotations;
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

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CreatedBy")]
        public required User Creator { get; set; }

        public required ICollection<EventCategory> EventCategories { get; set; }
        public required ICollection<Registration> Registrations { get; set; }
        public required ICollection<EventArea> EventAreas { get; set; }
        public required ICollection<Comment> Comments { get; set; }
    }
}
