using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanning.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set; }

        [Required]
        [MinLength(2)]
        public string WedderOne { get; set; }
        
        [Required]
        [MinLength(2)]
        public string WedderTwo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Where is your wedding?")]
        [MinLength(3)]
        public string Address { get; set; }
        public int UserId { get; set; }
        public User Planner { get; set; }
        public List<RSVP> GuestsAttending { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}