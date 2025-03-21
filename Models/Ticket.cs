﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementServer.Models
{
    public class Ticket
    {
        [Key]
        public int TicketID { get; set; }

        [Required]
        public int EventAreaID { get; set; }

        [Required, MaxLength(100)]
        public string TicketName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Unavailable"; // Available,  Unavailable , Sold Out

        [ForeignKey("EventAreaID")]
        public EventArea? EventArea { get; set; }

    }
}