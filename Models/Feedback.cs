using System;
using System.ComponentModel.DataAnnotations;

namespace Ceng382ClassroomReservation.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty; // "Feedback", "Support", "Bug"

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int Rating { get; set; } = 5;

    }
}
