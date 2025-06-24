using System.ComponentModel.DataAnnotations;

namespace Ceng382ClassroomReservation.Models
{
    public class Classroom
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Classroom Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
        public int Capacity { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
