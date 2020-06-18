using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace WeddingPlanning.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MinLength(2,ErrorMessage = "First Name must be at least 2 characters.")]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2,ErrorMessage = "Last Name must be at least 2 characters.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).{8,32}$",ErrorMessage="Password must be at least 8 characters and contain at least one uppercase character and number.")]
        public string Password { get; set; }

        [NotMapped]
        [Compare("Password", ErrorMessage="Passwords do not match.")]
        [DataType(DataType.Password)]
        public string Confirm { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<Wedding> WeddingsPlanned { get; set; }
        public List<RSVP> WeddingsAttending { get; set; }

    }
}